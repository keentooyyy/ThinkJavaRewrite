using UnityEngine;
using DG.Tweening;
using GameEvents;
using GameScoring;

namespace GameUI
{
    /// <summary>
    /// Reveals 1-3 stars with a pop animation using DOTween
    /// when the success UI event is triggered. Stars beyond
    /// the awarded count are dimmed.
    /// </summary>
    public class StarRevealDOTween : MonoBehaviour
    {
        [Header("Stars (order matters)")]
        [Tooltip("Assign 3 star transforms in left-to-right or first-to-last order")]
        [SerializeField] private Transform[] stars = new Transform[3];

        [Header("Animation")]
        [SerializeField] private float startDelay = 0.4f;
        [SerializeField] private float perStarDelay = 0.15f;
        [SerializeField] private float popDuration = 0.35f;
        [SerializeField] private float popScale = 1.2f;
        [SerializeField] private Ease popEase = Ease.OutBack;
        [SerializeField] private float inactiveAlpha = 0.25f;

        [Header("Events")]
        [Tooltip("Event fired when FSM computes score (ComputeLevelScore.onComputedEventName)")]
        [SerializeField] private string successEventName = "ScoreComputed";

        private Sequence sequence;

        private void Awake() { }

        private void OnEnable()
        {
            UIEventManager.Subscribe(successEventName, OnSuccess);
            PrepareInitialState();
        }

        private void OnDisable()
        {
            UIEventManager.Unsubscribe(successEventName, OnSuccess);
            if (sequence != null && sequence.IsActive())
            {
                sequence.Kill();
                sequence = null;
            }
        }

        private void PrepareInitialState()
        {
            // Set all stars visible but dimmed and at scale 1.
            for (int i = 0; i < stars.Length; i++)
            {
                var t = stars[i];
                if (t == null) continue;
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                var cg = GetOrAddCanvasGroup(t.gameObject);
                cg.alpha = inactiveAlpha;
                t.localScale = Vector3.one;
            }
        }

        private void OnSuccess()
        {
            int starsAwarded = Mathf.Clamp(LevelScoreRuntime.LastStars, 0, stars.Length);
            RevealStars(starsAwarded);
        }

        private void RevealStars(int starsAwarded)
        {
            if (sequence != null && sequence.IsActive())
            {
                sequence.Kill();
            }
            sequence = DOTween.Sequence();

            for (int i = 0; i < stars.Length; i++)
            {
                var t = stars[i];
                if (t == null) continue;
                var cg = GetOrAddCanvasGroup(t.gameObject);

                if (i < starsAwarded)
                {
                    // Start hidden/small
                    t.localScale = Vector3.zero;
                    cg.alpha = 0f;

                    var s = DOTween.Sequence();
                    s.AppendInterval(startDelay + i * perStarDelay);
                    s.Append(t.DOScale(popScale, popDuration * 0.6f).SetEase(popEase));
                    s.Join(cg.DOFade(1f, popDuration * 0.6f));
                    s.Append(t.DOScale(1f, popDuration * 0.4f).SetEase(Ease.OutQuad));
                    sequence.Join(s);
                }
                else
                {
                    // Keep dimmed for non-awarded
                    t.localScale = Vector3.one;
                    cg.alpha = inactiveAlpha;
                }
            }
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
