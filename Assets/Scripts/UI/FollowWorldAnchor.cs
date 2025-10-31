using DG.Tweening;
using UnityEngine;

namespace GameUI
{
    /// <summary>
    /// Smoothly follows a world-space anchor (e.g. the player's PromptAnchor)
    /// and optionally adds a bobbing offset for UI prompts.
    /// </summary>
    [DisallowMultipleComponent]
    public class FollowWorldAnchor : MonoBehaviour
    {
        [Header("Anchor Lookup")]
        [Tooltip("Name of the child transform under the tagged object (e.g. PromptAnchor)")]
        public string anchorName = "PromptAnchor";

        [Tooltip("Tag of the GameObject that owns the anchor (usually Player)")]
        public string parentTag = "Player";

        [Tooltip("Optional: re-check for the anchor every X seconds if lost. Set to 0 to disable.")]
        [Min(0f)] public float requeryInterval = 1f;

        [Header("Follow Smoothing")]
        [Tooltip("Smoothly tween towards the anchor position instead of snapping")]
        public bool smoothFollow = true;

        [Tooltip("Seconds for the follow tween to reach the target position")]
        [Min(0.01f)] public float followDuration = 0.12f;

        public Ease followEase = Ease.OutQuad;

        [Tooltip("Should the tween ignore timeScale? Useful for pause screens.")]
        public bool useUnscaledTime = true;

        [Header("Bobbing")]
        [Tooltip("Add a vertical bobbing motion on top of the anchor position")]
        public bool enableBobbing = true;

        [Tooltip("Peak amplitude of the bob (UI units)")]
        public float bobAmplitude = 8f;

        [Tooltip("Bobbing cycles per second")]
        public float bobFrequency = 1.2f;

        [Tooltip("Optional phase offset for bobbing (seconds)")]
        public float bobPhaseOffset = 0f;

        private Transform anchor;
        private Canvas canvas;
        private RectTransform rectTransform;
        private Camera mainCamera;
        private Tween followTween;
        private Vector2 currentSmoothedPosition;
        private Vector2 lastTargetPosition;
        private bool hasTarget;
        private float nextAnchorLookupTime;

        private bool IsOverlay => canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay;
        private float TimeNow => useUnscaledTime ? Time.unscaledTime : Time.time;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            FindAnchor();
        }

        private void OnDisable()
        {
            followTween?.Kill();
            followTween = null;
            hasTarget = false;
        }

        private void LateUpdate()
        {
            if (anchor == null)
            {
                // Optionally retry finding the anchor
                if (requeryInterval <= 0f)
                    return;

                if (TimeNow >= nextAnchorLookupTime)
                {
                    FindAnchor();
                    nextAnchorLookupTime = TimeNow + requeryInterval;
                }
                return;
            }

            Vector2 targetPosition;
            if (IsOverlay)
            {
                Vector3 screenPos = mainCamera != null
                    ? mainCamera.WorldToScreenPoint(anchor.position)
                    : Vector3.zero;
                targetPosition = screenPos;
            }
            else
            {
                Vector2 canvasPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    mainCamera.WorldToScreenPoint(anchor.position),
                    canvas.worldCamera,
                    out canvasPos);
                targetPosition = canvasPos;
            }

            UpdateSmoothedPosition(targetPosition);
            ApplyFinalPosition();
        }

        private void UpdateSmoothedPosition(Vector2 target)
        {
            if (!smoothFollow)
            {
                currentSmoothedPosition = target;
                followTween?.Kill();
                followTween = null;
                hasTarget = true;
                lastTargetPosition = target;
                return;
            }

            if (!hasTarget)
            {
                currentSmoothedPosition = target;
                hasTarget = true;
                lastTargetPosition = target;
                return;
            }

            // Avoid starting new tweens for tiny movements
            if ((target - lastTargetPosition).sqrMagnitude < 0.25f)
            {
                lastTargetPosition = target;
                return;
            }

            followTween?.Kill();
            followTween = DOTween.To(() => currentSmoothedPosition, value => currentSmoothedPosition = value, target, followDuration)
                                 .SetEase(followEase)
                                 .SetUpdate(useUnscaledTime);

            lastTargetPosition = target;
        }

        private void ApplyFinalPosition()
        {
            Vector2 bobOffset = CalculateBobOffset();
            Vector2 final = currentSmoothedPosition + bobOffset;

            if (IsOverlay)
            {
                rectTransform.position = new Vector3(final.x, final.y, rectTransform.position.z);
            }
            else
            {
                rectTransform.anchoredPosition = final;
            }
        }

        private Vector2 CalculateBobOffset()
        {
            if (!enableBobbing || bobAmplitude <= 0f || bobFrequency <= 0f)
                return Vector2.zero;

            float t = (TimeNow + bobPhaseOffset) * bobFrequency * Mathf.PI * 2f;
            float bobY = Mathf.Sin(t) * bobAmplitude;
            return new Vector2(0f, bobY);
        }

        private void FindAnchor()
        {
            GameObject parent = GameObject.FindGameObjectWithTag(parentTag);
            if (parent == null)
            {
                Debug.LogWarning($"FollowWorldAnchor: GameObject with tag '{parentTag}' not found!");
                return;
            }

            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == anchorName)
                {
                    anchor = child;
                    hasTarget = false; // force snap to new anchor on next update
                    return;
                }
            }

            Debug.LogWarning($"FollowWorldAnchor: '{anchorName}' not found under {parent.name}!");
        }
    }
}
