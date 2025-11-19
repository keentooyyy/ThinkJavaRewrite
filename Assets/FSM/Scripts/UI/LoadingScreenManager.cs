using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using GameCore;

namespace GameUI
{
    /// <summary>
    /// Manages the loading screen with shader/TMP warming and async scene loading.
    /// Progress bar container moves from left to right with skip/jump effects.
    /// </summary>
    public class LoadingScreenManager : MonoBehaviour
    {
        [Header("Progress Bar")]
        [Tooltip("The progress bar container RectTransform (moves from left to right). Player should be parented to this.")]
        [SerializeField] private RectTransform progressBarContainer;

        [Tooltip("Optional: Text component to show percentage (e.g., '45%')")]
        [SerializeField] private TMP_Text progressText;

        [Header("Loading Content")]
        [Tooltip("ScriptableObject containing quotes and images to display during loading")]
        [SerializeField] private LoadingScreenContent loadingContent;

        [Tooltip("Text component to display motivational quotes")]
        [SerializeField] private TMP_Text quoteText;

        [Tooltip("Text component to display quote author name (e.g., 'Mark Zuckerberg')")]
        [SerializeField] private TMP_Text authorText;

        [Tooltip("Image component to display banner/sprites (swaps during loading)")]
        [SerializeField] private UnityEngine.UI.Image bannerImage;

        [Header("Player Animation")]
        [Tooltip("Player character GameObject to animate during loading (running animation). Should be parented to progress bar container.")]
        [SerializeField] private GameObject playerCharacter;

        [Tooltip("Player animation speed multiplier (1 = normal speed, higher = faster)")]
        [SerializeField] private float playerAnimationSpeed = 1f;

        [Tooltip("Starting X position offset (off-screen left). Progress bar will start here and move to final position.")]
        [SerializeField] private float startXOffset = -2000f;

        [Tooltip("Number of skip steps (big jumps) during loading. Higher = more frequent skips.")]
        [SerializeField] private int numberOfSkips = 8;

        [Tooltip("Skip randomness (0 = even steps, higher = more random timing)")]
        [SerializeField] private float skipRandomness = 0.3f;


        [Header("Loading Settings")]
        [Tooltip("Total loading duration (in seconds). You control this to show lore/loading tips.")]
        [SerializeField] private float loadingDuration = 3f;

        [Header("Shader Warming")]
        [Tooltip("Enable shader warming to prevent first-frame stutter")]
        [SerializeField] private bool warmShaders = true;

        [Tooltip("Shader names to warm up (common Unity and TMP shaders)")]
        [SerializeField] private string[] shaderNames = new string[]
        {
            "TextMeshPro/Distance Field",
            "TextMeshPro/Mobile/Distance Field",
            "TextMeshPro/Sprite",
            "UI/Default",
            "Sprites/Default",
            "Unlit/Texture"
        };


        private float currentProgress = 0f;
        private AsyncOperation sceneAsyncOperation = null;
        private Animator playerAnimator = null;
        private Vector2 progressBarFinalPosition = Vector2.zero;
        private float[] skipPoints = null;
        private Coroutine contentUpdateCoroutine = null;

        private void Start()
        {
            // Store final position of progress bar (where you placed it in editor)
            if (progressBarContainer != null)
            {
                progressBarFinalPosition = progressBarContainer.anchoredPosition;
                // Set initial position (off-screen left)
                Vector2 startPos = progressBarFinalPosition;
                startPos.x += startXOffset;
                progressBarContainer.anchoredPosition = startPos;
                
                // Generate skip points for chunked movement
                GenerateSkipPoints();
            }

            if (progressText != null)
            {
                progressText.text = "0%";
            }

            // Get player animator reference (you handle animation setup yourself)
            if (playerCharacter != null)
            {
                playerAnimator = playerCharacter.GetComponent<Animator>();
                if (playerAnimator != null)
                {
                    // Just set animation speed multiplier
                    playerAnimator.speed = playerAnimationSpeed;
                }
            }

            // Start content display (quotes and images)
            if (loadingContent != null)
            {
                contentUpdateCoroutine = StartCoroutine(UpdateContentCoroutine());
            }

            // Start loading process
            StartCoroutine(LoadingCoroutine());
        }

        private void OnDestroy()
        {
            if (contentUpdateCoroutine != null)
            {
                StopCoroutine(contentUpdateCoroutine);
            }
        }


        private IEnumerator LoadingCoroutine()
        {
            float startTime = Time.time;
            string targetScene = SceneLoader.GetAndClearTargetScene();

            if (string.IsNullOrEmpty(targetScene))
            {
                Debug.LogError("LoadingScreenManager: No target scene specified! Loading MainMenu as fallback.");
                targetScene = "MainMenu";
            }

            // Start loading scene in background
            sceneAsyncOperation = SceneManager.LoadSceneAsync(targetScene);
            sceneAsyncOperation.allowSceneActivation = false; // Don't activate until we're done

            // Warm up shaders and TMP (happens during loading duration)
            if (warmShaders)
            {
                yield return StartCoroutine(WarmUpShadersAndTMP());
            }

            // Run loading animation for the specified duration
            // This gives you time to show lore/loading tips
            float elapsed = 0f;
            while (elapsed < loadingDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / loadingDuration);
                UpdateProgress(progress);
                yield return null;
            }

            // Ensure we're at 100%
            UpdateProgress(1f);

            // Wait for scene to be ready (if it's not already)
            while (sceneAsyncOperation.progress < 0.9f)
            {
                yield return null;
            }

            // Small delay before transitioning
            yield return new WaitForSeconds(0.2f);

            // Activate the scene
            if (sceneAsyncOperation != null)
            {
                sceneAsyncOperation.allowSceneActivation = true;
            }
        }

        private IEnumerator WarmUpShadersAndTMP()
        {
            // Warm up shaders
            if (shaderNames != null && shaderNames.Length > 0)
            {
                foreach (string shaderName in shaderNames)
                {
                    Shader shader = Shader.Find(shaderName);
                    if (shader != null)
                    {
                        // Create a temporary material to warm up the shader
                        Material tempMaterial = new Material(shader);
                        // Force shader compilation by setting a property
                        tempMaterial.SetFloat("_MainTex", 0f);
                        Destroy(tempMaterial);
                    }
                    yield return null;
                }
            }

            // Warm up TMP by creating a temporary text object
            GameObject tempTMP = new GameObject("TempTMP");
            TMP_Text tmpText = tempTMP.AddComponent<TextMeshProUGUI>();
            tmpText.text = "Warming up...";
            tmpText.fontSize = 12;
            // Force TMP to initialize
            tmpText.ForceMeshUpdate();
            yield return null;
            Destroy(tempTMP);
        }

        private void UpdateProgress(float progress)
        {
            currentProgress = Mathf.Clamp01(progress);

            // Update progress text
            if (progressText != null)
            {
                int percentage = Mathf.RoundToInt(currentProgress * 100f);
                progressText.text = $"{percentage}%";
            }

            // Update player animation speed based on progress (optional enhancement)
            // This makes the player run faster as loading progresses
            if (playerAnimator != null)
            {
                float speedMultiplier = 1f + (currentProgress * 0.5f); // 1x to 1.5x speed
                playerAnimator.speed = playerAnimationSpeed * speedMultiplier;
            }

            // Move progress bar container from left to right with skips/jumps (player moves with it since it's parented)
            if (progressBarContainer != null && skipPoints != null && skipPoints.Length > 0)
            {
                Vector2 startPos = progressBarFinalPosition;
                startPos.x += startXOffset;
                
                // Find which skip point we're at
                float targetProgress = GetSkippedProgress(currentProgress);
                Vector2 currentPos = Vector2.Lerp(startPos, progressBarFinalPosition, targetProgress);
                progressBarContainer.anchoredPosition = currentPos;
            }
        }

        private void GenerateSkipPoints()
        {
            // Generate random skip points between 0 and 1
            skipPoints = new float[numberOfSkips + 2]; // +2 for start (0) and end (1)
            skipPoints[0] = 0f;
            skipPoints[skipPoints.Length - 1] = 1f;
            
            for (int i = 1; i < skipPoints.Length - 1; i++)
            {
                float baseProgress = (float)i / (numberOfSkips + 1);
                float randomOffset = (Random.Range(-skipRandomness, skipRandomness) / (numberOfSkips + 1));
                skipPoints[i] = Mathf.Clamp01(baseProgress + randomOffset);
            }
            
            // Sort to ensure they're in order
            System.Array.Sort(skipPoints);
        }

        private float GetSkippedProgress(float progress)
        {
            if (skipPoints == null || skipPoints.Length == 0)
                return progress;
            
            // Find which two skip points we're between
            for (int i = 0; i < skipPoints.Length - 1; i++)
            {
                if (progress >= skipPoints[i] && progress <= skipPoints[i + 1])
                {
                    // Snap to the next skip point when we pass the threshold
                    // This creates the "jump" effect
                    float threshold = skipPoints[i] + ((skipPoints[i + 1] - skipPoints[i]) * 0.3f);
                    if (progress >= threshold)
                    {
                        return skipPoints[i + 1];
                    }
                    else
                    {
                        return skipPoints[i];
                    }
                }
            }
            
            return progress;
        }

        private IEnumerator UpdateContentCoroutine()
        {
            if (loadingContent == null)
                yield break;

            while (true)
            {
                // Pick random quote
                if (loadingContent.quotes != null && loadingContent.quotes.Count > 0)
                {
                    var quote = loadingContent.quotes[Random.Range(0, loadingContent.quotes.Count)];
                    if (quoteText != null)
                    {
                        quoteText.text = quote.quoteText;
                    }
                    if (authorText != null)
                    {
                        string authorDisplay = quote.authorName;
                        if (!string.IsNullOrEmpty(quote.authorTitle))
                        {
                            authorDisplay += $" - {quote.authorTitle}";
                        }
                        authorText.text = authorDisplay;
                    }
                }

                // Pick random banner image
                if (loadingContent.bannerImages != null && loadingContent.bannerImages.Count > 0)
                {
                    if (bannerImage != null)
                    {
                        bannerImage.sprite = loadingContent.bannerImages[Random.Range(0, loadingContent.bannerImages.Count)];
                        bannerImage.enabled = true;
                    }
                }
                else if (bannerImage != null)
                {
                    bannerImage.enabled = false;
                }

                // Wait for display duration before showing next random content
                yield return new WaitForSeconds(loadingContent.displayDuration);
            }
        }
    }
}

