using UnityEngine;
using DG.Tweening;
using GameEvents;

namespace UI
{
    /// <summary>
    /// Animates UI elements with a bouncy scale effect using DOTween
    /// Excludes "Background" GameObject from animation
    /// </summary>
    public class UIScaleBounceAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Duration of the animation")]
        public float duration = 0.5f;
        
        [Tooltip("How much overshoot/bounce. Higher = more bounce")]
        [Range(0f, 2f)]
        public float overshoot = 1.7f;
        
        [Tooltip("Delay before starting animation")]
        public float delay = 0f;
        
        [Header("Show Animation")]
        [Tooltip("Event name to trigger show animation")]
        public string showEventName = "ShowLoginUI";
        
        [Tooltip("Animate on start (when GameObject is enabled)")]
        public bool animateOnStart = true;
        
        [Header("Hide Animation")]
        [Tooltip("Event name to trigger hide animation")]
        public string hideEventName = "HideLoginUI";
        
        [Tooltip("Disable GameObject after hide animation completes")]
        public bool disableAfterHide = true;
        
        [Tooltip("Hide animation duration multiplier (0.5 = twice as fast)")]
        [Range(0.1f, 1f)]
        public float hideDurationMultiplier = 0.5f;
        
        [Header("Exclusions")]
        [Tooltip("GameObject names to exclude from animation (e.g. 'Background')")]
        public string[] excludedGameObjects = new string[] { "Background" };
        
        private Transform[] animatedTransforms;
        private Vector3[] originalScales;
        
        private void Awake()
        {
            // Find all child transforms except excluded ones
            var allChildren = GetComponentsInChildren<Transform>(true);
            var animatedList = new System.Collections.Generic.List<Transform>();
            var scalesList = new System.Collections.Generic.List<Vector3>();
            
            foreach (var child in allChildren)
            {
                // Skip self
                if (child == transform) continue;
                
                // Check if this GameObject should be excluded
                bool shouldExclude = false;
                foreach (var excluded in excludedGameObjects)
                {
                    if (child.name == excluded)
                    {
                        shouldExclude = true;
                        break;
                    }
                }
                
                if (!shouldExclude)
                {
                    animatedList.Add(child);
                    scalesList.Add(child.localScale);
                }
            }
            
            animatedTransforms = animatedList.ToArray();
            originalScales = scalesList.ToArray();
        }
        
        private void OnEnable()
        {
            // Subscribe to events
            if (!string.IsNullOrEmpty(showEventName))
            {
                UIEventManager.Subscribe(showEventName, Show);
            }
            
            if (!string.IsNullOrEmpty(hideEventName))
            {
                UIEventManager.Subscribe(hideEventName, Hide);
            }
            
            // Play show animation on start if enabled
            if (animateOnStart)
            {
                Show();
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            if (!string.IsNullOrEmpty(showEventName))
            {
                UIEventManager.Unsubscribe(showEventName, Show);
            }
            
            if (!string.IsNullOrEmpty(hideEventName))
            {
                UIEventManager.Unsubscribe(hideEventName, Hide);
            }
        }
        
        /// <summary>
        /// Show animation: Scale from 0 to original with bounce
        /// </summary>
        public void Show()
        {
            if (animatedTransforms == null || animatedTransforms.Length == 0) return;
            
            // Kill any ongoing animations
            foreach (var t in animatedTransforms)
            {
                if (t != null) DOTween.Kill(t);
            }
            
            // Set all to scale 0
            for (int i = 0; i < animatedTransforms.Length; i++)
            {
                if (animatedTransforms[i] != null)
                    animatedTransforms[i].localScale = Vector3.zero;
            }
            
            // Animate each to original scale with bounce
            for (int i = 0; i < animatedTransforms.Length; i++)
            {
                if (animatedTransforms[i] == null) continue;
                
                int index = i;
                animatedTransforms[i]
                    .DOScale(originalScales[index], duration)
                    .SetEase(Ease.OutBack, overshoot)
                    .SetUpdate(true);
            }
        }
        
        /// <summary>
        /// Hide animation: Scale from current to 0
        /// </summary>
        public void Hide()
        {
            float hideDuration = duration * hideDurationMultiplier;
            
            if (animatedTransforms == null || animatedTransforms.Length == 0)
            {
                return;
            }
            
            // Kill any ongoing animations on all animated transforms
            foreach (var t in animatedTransforms)
            {
                if (t != null) DOTween.Kill(t);
            }
            
            // Animate each to scale 0
            for (int i = 0; i < animatedTransforms.Length; i++)
            {
                if (animatedTransforms[i] == null) continue;
                
                animatedTransforms[i]
                    .DOScale(Vector3.zero, hideDuration)
                    .SetEase(Ease.InBack, overshoot)
                    .SetUpdate(true);
            }
            
            // Disable GameObject after animation if requested
            if (disableAfterHide)
            {
                DOVirtual.DelayedCall(hideDuration + 0.05f, () =>
                {
                    gameObject.SetActive(false);
                }).SetUpdate(true);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up any ongoing animations
            DOTween.Kill(transform);
        }
    }
}

