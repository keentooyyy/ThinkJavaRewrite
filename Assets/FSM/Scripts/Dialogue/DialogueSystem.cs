using System;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Dialogue;
using GameEvents;
using GameInput;
using GameState;


namespace DialogueRuntime
{
    /// <summary>
    /// Handles dialogue playback: typewriter reveal, skip-to-end, and line progression.
    /// Expects UI references to be wired in the inspector.
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private CanvasGroup container;
        [SerializeField] private TextMeshProUGUI speakerLabel;
        [SerializeField] private TextMeshProUGUI bodyLabel;
        [SerializeField] private GameObject continueIndicator;

        [Header("Behaviour")]
        [SerializeField] private string confirmButton = "ActionA";
        [Tooltip("Default characters revealed per second when a line does not override the speed.")]
        [SerializeField] private float defaultLettersPerSecond = 30f;
        [Tooltip("Minimum time (seconds) a line should take to reveal, even if it is short.")]
        [SerializeField] private float minimumRevealDuration = 0.1f;
        [SerializeField] private Ease revealEase = Ease.Linear;
        [SerializeField] private AudioSource typeAudioSource;
        [SerializeField] private AudioClip typeAudioClip;
        [Tooltip("Play the type SFX every N revealed characters. Use 0 or 1 to play on each new character.")]
        [SerializeField] private int charactersPerSfx = 2;
        [Tooltip("Randomize pitch in this range when playing the type SFX. Set both to 1 for constant pitch.")]
        [SerializeField] private Vector2 typePitchRange = new Vector2(1f, 1f);

        [Header("Events (fired through UIEventManager)")]
        [SerializeField] private string dialogueStartEvent = "DialogueStart";
        [SerializeField] private string dialogueEndEvent = "DialogueEnd";
        [SerializeField] private string dialogueLineCompleteEvent = "DialogueLineComplete";

        private DialogueSequence activeSequence;
        private int currentIndex;
        private Tween revealTween;
        private bool isRevealing;
        private bool isActive;
        private Action onDialogueFinished;
        private int lastSoundCharIndex;

        public bool IsActive => isActive;
        public bool IsRevealing => isRevealing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Multiple DialogueSystem instances detected. Using the instance on '{Instance.gameObject.name}'.");
                enabled = false;
                return;
            }

            Instance = this;
            SetContainerVisible(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            revealTween?.Kill();
            revealTween = null;
            lastSoundCharIndex = -1;
        }

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            if (InputManager.GetButtonDown(confirmButton))
            {
                Advance();
            }
        }

        /// <summary>
        /// Start a new dialogue sequence. Optionally provide a callback for when the dialogue ends.
        /// </summary>
        public void BeginDialogue(DialogueSequence sequence, Action onComplete = null)
        {
            if (sequence == null || sequence.LineCount == 0)
            {
                Debug.LogWarning("DialogueSystem received an empty sequence.");
                onComplete?.Invoke();
                return;
            }

            if (isActive)
            {
                ForceCompleteDialogue();
            }

            activeSequence = sequence;
            currentIndex = 0;
            onDialogueFinished = onComplete;
            isActive = true;

            SetContainerVisible(true);
            PlayCurrentLine();

            GameFreezeManager.SetFreeze(GameFreezeType.Dialogue);
            if (!string.IsNullOrEmpty(dialogueStartEvent))
            {
                UIEventManager.Trigger(dialogueStartEvent);
            }
        }

        /// <summary>
        /// Immediately finishes the current reveal. If no reveal is running, progresses to the next line or ends the dialogue.
        /// </summary>
        public void Advance()
        {
            if (!isActive)
            {
                return;
            }

            if (isRevealing)
            {
                CompleteRevealTween();
                return;
            }

            currentIndex++;
            if (activeSequence == null || currentIndex >= activeSequence.LineCount)
            {
                FinishDialogue();
            }
            else
            {
                PlayCurrentLine();
            }
        }

        /// <summary>
        /// Cancels the current dialogue, skipping events. Use ForceCompleteDialogue to ensure callbacks fire.
        /// </summary>
        public void CancelDialogue()
        {
            if (!isActive)
            {
                return;
            }

            revealTween?.Kill();
            revealTween = null;
            isActive = false;
            isRevealing = false;
            activeSequence = null;
            SetContainerVisible(false);
            continueIndicator?.SetActive(false);
            onDialogueFinished = null;
            GameFreezeManager.ClearFreeze();
        }

        /// <summary>
        /// Ends the dialogue immediately, firing completion callbacks and events.
        /// </summary>
        public void ForceCompleteDialogue()
        {
            if (!isActive)
            {
                return;
            }

            revealTween?.Kill(true);
            revealTween = null;
            FinishDialogue();
        }

        private void PlayCurrentLine()
        {
            if (activeSequence == null)
            {
                FinishDialogue();
                return;
            }

            var line = activeSequence.GetLine(currentIndex) ?? new DialogueLine();

            if (speakerLabel != null)
            {
                speakerLabel.text = line.Speaker;
                speakerLabel.gameObject.SetActive(!string.IsNullOrEmpty(line.Speaker));
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = string.Empty;
            }

            continueIndicator?.SetActive(false);

            float lettersPerSecond = line.LettersPerSecond > 0f ? line.LettersPerSecond : defaultLettersPerSecond;
            float duration = lettersPerSecond > 0f
                ? Mathf.Max(minimumRevealDuration, line.Text.Length / lettersPerSecond)
                : minimumRevealDuration;

            revealTween?.Kill();
            isRevealing = true;

            if (bodyLabel == null)
            {
                Debug.LogWarning("DialogueSystem: No body label assigned.");
                CompleteRevealImmediately();
                return;
            }

            revealTween = bodyLabel
                .DOText(line.Text ?? string.Empty, duration, richTextEnabled: true, scrambleMode: ScrambleMode.None)
                .SetEase(revealEase)
                .OnStart(() => lastSoundCharIndex = -1)
                .OnUpdate(HandleTypeSound)
                .OnComplete(OnRevealComplete);
        }

        private void OnRevealComplete()
        {
            isRevealing = false;
            continueIndicator?.SetActive(true);

            if (!string.IsNullOrEmpty(dialogueLineCompleteEvent))
            {
                UIEventManager.Trigger(dialogueLineCompleteEvent);
            }
        }

        private void CompleteRevealTween()
        {
            if (revealTween != null && revealTween.IsActive())
            {
                revealTween.Complete(true);
            }
            else
            {
                CompleteRevealImmediately();
            }
        }

        private void CompleteRevealImmediately()
        {
            isRevealing = false;

            if (bodyLabel != null && activeSequence != null)
            {
                var line = activeSequence.GetLine(currentIndex);
                bodyLabel.text = line != null ? line.Text : string.Empty;
            }
            OnRevealComplete();
        }

        private void HandleTypeSound()
        {
            if (typeAudioSource == null || typeAudioClip == null || bodyLabel == null)
            {
                return;
            }

            int currentVisible = bodyLabel.maxVisibleCharacters;
            if (currentVisible <= lastSoundCharIndex)
            {
                return;
            }

            int step = Mathf.Max(1, charactersPerSfx);
            if (currentVisible % step != 0)
            {
                return;
            }

            if (typePitchRange.x != 1f || typePitchRange.y != 1f)
            {
                typeAudioSource.pitch = UnityEngine.Random.Range(typePitchRange.x, typePitchRange.y);
            }
            else
            {
                typeAudioSource.pitch = 1f;
            }

            typeAudioSource.PlayOneShot(typeAudioClip);
            lastSoundCharIndex = currentVisible;
        }

        private void FinishDialogue()
        {
            revealTween?.Kill();
            revealTween = null;

            isActive = false;
            isRevealing = false;
            activeSequence = null;
            SetContainerVisible(false);
            continueIndicator?.SetActive(false);
            lastSoundCharIndex = -1;

            GameFreezeManager.ClearFreeze();

            if (!string.IsNullOrEmpty(dialogueEndEvent))
            {
                UIEventManager.Trigger(dialogueEndEvent);
            }

            var callback = onDialogueFinished;
            onDialogueFinished = null;
            callback?.Invoke();
        }

        private void SetContainerVisible(bool visible)
        {
            if (container != null)
            {
                container.gameObject.SetActive(true);
                container.alpha = visible ? 1f : 0f;
                container.interactable = visible;
                container.blocksRaycasts = visible;
            }
        }
    }
}
