using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    /// <summary>
    /// Simple progress bar component that fills from left to right.
    /// Can be used standalone or with LoadingScreenManager.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ProgressBar : MonoBehaviour
    {
        [Header("Progress Bar")]
        [Tooltip("The Image component that will be filled. Auto-detected if not assigned.")]
        [SerializeField] private Image fillImage;

        [Tooltip("Optional: Text component to show percentage")]
        [SerializeField] private TMPro.TMP_Text percentageText;

        [Header("Animation")]
        [Tooltip("Smooth animation speed (0 = instant, higher = slower)")]
        [SerializeField] private float animationSpeed = 5f;

        private float targetProgress = 0f;
        private float currentProgress = 0f;

        private void Awake()
        {
            if (fillImage == null)
            {
                fillImage = GetComponent<Image>();
            }

            if (fillImage != null)
            {
                // Configure image for horizontal fill from left
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = 0; // Left
                fillImage.fillAmount = 0f;
            }
        }

        private void Update()
        {
            if (fillImage == null) return;

            // Smoothly animate to target progress
            if (Mathf.Abs(currentProgress - targetProgress) > 0.001f)
            {
                currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * animationSpeed);
                fillImage.fillAmount = currentProgress;

                // Update percentage text if assigned
                if (percentageText != null)
                {
                    int percentage = Mathf.RoundToInt(currentProgress * 100f);
                    percentageText.text = $"{percentage}%";
                }
            }
        }

        /// <summary>
        /// Set the progress (0-1)
        /// </summary>
        public void SetProgress(float progress)
        {
            targetProgress = Mathf.Clamp01(progress);
        }

        /// <summary>
        /// Set the progress instantly (no animation)
        /// </summary>
        public void SetProgressInstant(float progress)
        {
            targetProgress = Mathf.Clamp01(progress);
            currentProgress = targetProgress;
            
            if (fillImage != null)
            {
                fillImage.fillAmount = currentProgress;
            }

            if (percentageText != null)
            {
                int percentage = Mathf.RoundToInt(currentProgress * 100f);
                percentageText.text = $"{percentage}%";
            }
        }

        /// <summary>
        /// Get current progress (0-1)
        /// </summary>
        public float GetProgress()
        {
            return currentProgress;
        }
    }
}

