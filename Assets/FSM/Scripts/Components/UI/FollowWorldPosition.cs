using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Makes UI element follow a world position (player anchor)
    /// Auto-finds player by tag and child transform by name
    /// No scene-specific references needed!
    /// </summary>
    public class FollowWorldPosition : MonoBehaviour
    {
        [Header("Auto-Find Settings")]
        [Tooltip("Tag to find player (e.g., 'Player')")]
        public string playerTag = "Player";
        
        [Tooltip("Name of child transform on player to follow (e.g., 'PromptAnchor')")]
        public string anchorChildName = "PromptAnchor";
        
        [Header("Optional Manual Override")]
        [Tooltip("If set, ignores auto-find and uses this transform")]
        public Transform manualTarget;
        
        [Header("Offset")]
        [Tooltip("Additional offset from anchor position")]
        public Vector3 offset = Vector3.zero;
        
        private Transform worldTarget;
        private RectTransform rectTransform;
        private Canvas canvas;
        private Camera mainCamera;
        
        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                Debug.LogWarning("FollowWorldPosition: No main camera found!");
            }
            
            // Find target at runtime
            FindTarget();
        }
        
        private void FindTarget()
        {
            // Use manual override if set
            if (manualTarget != null)
            {
                worldTarget = manualTarget;
                Debug.Log($"FollowWorldPosition: Using manual target {worldTarget.name}");
                return;
            }
            
            // Find player by tag
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player == null)
            {
                Debug.LogWarning($"FollowWorldPosition: No GameObject with tag '{playerTag}' found!");
                return;
            }
            
            // Find child by name
            Transform anchor = player.transform.Find(anchorChildName);
            if (anchor != null)
            {
                worldTarget = anchor;
                Debug.Log($"FollowWorldPosition: Following {player.name} → {anchorChildName}");
            }
            else
            {
                // Fallback to player root if child not found
                worldTarget = player.transform;
                Debug.LogWarning($"FollowWorldPosition: Child '{anchorChildName}' not found on {player.name}, using root transform");
            }
        }
        
        private void Update()
        {
            // Only update if this UI is active and we have a valid target
            if (!gameObject.activeSelf || worldTarget == null || mainCamera == null) 
                return;
            
            // Convert world position to screen position
            Vector3 targetPos = worldTarget.position + offset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetPos);
            
            // Update UI position based on canvas render mode
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                rectTransform.position = screenPos;
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector2 canvasPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    screenPos,
                    canvas.worldCamera,
                    out canvasPos
                );
                rectTransform.localPosition = canvasPos;
            }
        }
        
        // Optional: Allow re-finding target at runtime (if player respawns, etc.)
        public void RefindTarget()
        {
            FindTarget();
        }
    }
}

