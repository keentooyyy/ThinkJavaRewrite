using UnityEngine;
using TMPro;
using DG.Tweening;
using GameEvents;
using GameScoring;

namespace GameUI
{
    /// <summary>
    /// Animates the score text from 000 to final value when the success UI event is triggered.
    /// </summary>
    [DisallowMultipleComponent]
    public class ScoreCounterDOTween : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private string numberFormat = "000";

        [Header("Animation")]
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        [Header("Events")]
        [Tooltip("Event fired when FSM computes score (ComputeLevelScore.onComputedEventName)")]
        [SerializeField] private string successEventName = "ScoreComputed";
        [Tooltip("Event fired AFTER the score has finished animating (used to chain star reveal)")]
        [SerializeField] private string onCompleteEventName = "ScoreCounted";

        private Tween activeTween;

        private void Reset()
        {
            TryResolveText();
        }

        private void Awake()
        {
            TryResolveText();
        }

        private void OnEnable()
        {
            UIEventManager.Subscribe(successEventName, OnSuccess);
            TryResolveText();
            if (scoreText != null)
            {
                scoreText.text = 0.ToString(numberFormat);
            }
        }

        private void OnDisable()
        {
            UIEventManager.Unsubscribe(successEventName, OnSuccess);
            if (activeTween != null && activeTween.IsActive())
            {
                activeTween.Kill();
                activeTween = null;
            }
        }

        private void OnSuccess()
        {
            TryResolveText();
            int target = LevelScoreRuntime.LastScore;
            AnimateTo(target);
        }

        private void AnimateTo(int target)
        {
            if (activeTween != null && activeTween.IsActive())
            {
                activeTween.Kill();
            }

            int current = 0;
            if (scoreText != null && int.TryParse(scoreText.text, out var parsed))
            {
                current = parsed;
            }

            int value = current;
            activeTween = DOTween.To(() => value, v =>
            {
                value = v;
                if (scoreText != null)
                {
                    scoreText.text = value.ToString(numberFormat);
                }
            }, target, duration)
            .SetEase(ease)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (scoreText != null)
                {
                    scoreText.text = target.ToString(numberFormat);
                }
                if (!string.IsNullOrEmpty(onCompleteEventName))
                {
                    UIEventManager.Trigger(onCompleteEventName);
                }
            });
        }

        private void TryResolveText()
        {
            if (scoreText != null) return;

            // Prefer a TMP_Text on the same GameObject
            scoreText = GetComponent<TMP_Text>();
            if (scoreText != null) return;

            // Fallback: search children (include inactive)
            scoreText = GetComponentInChildren<TMP_Text>(true);
        }
    }
}
