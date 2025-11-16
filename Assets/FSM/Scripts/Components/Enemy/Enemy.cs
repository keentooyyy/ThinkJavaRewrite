using UnityEngine;
using GameHazards;
using System.Collections;

namespace GameEnemies
{
    /// <summary>
    /// Simple enemy component that registers itself with the global enemy manager.
    /// Each enemy has its own waypoints and settings.
    /// </summary>
    [AddComponentMenu("Game/Enemy")]
    public class Enemy : MonoBehaviour
    {
        [Header("Waypoints")]
        [Tooltip("Point A - starting waypoint")]
        [SerializeField] private GameObject pointA;

        [Tooltip("Point B - ending waypoint")]
        [SerializeField] private GameObject pointB;

        [Header("Enemy Settings")]
        [Tooltip("Movement speed between waypoints")]
        [SerializeField] private float moveSpeed = 3f;

        [Tooltip("Head stomp detector (should be on head collider child)")]
        [SerializeField] private EnemyHeadStompDetector headStompDetector;

        [Header("Runtime State")]
        [SerializeField] private bool isDead = false;
        [SerializeField] private bool wasStomped = false;
        
        
        [Header("Stomp Settings")]
        [Tooltip("Delay before disabling colliders after being stomped (allows player to land properly)")]
        [SerializeField] private float colliderDisableDelay = 0.1f;
        
        // Cached components for disabling on stomp
        private Hazard hazardComponent;
        private Hazard[] hazardComponentsInChildren;
        private Collider2D[] bodyColliders;
        private Coroutine disableCollidersCoroutine;

        public GameObject PointA => pointA;
        public GameObject PointB => pointB;
        public float MoveSpeed => moveSpeed;
        public EnemyHeadStompDetector HeadStompDetector => headStompDetector;
        public bool IsDead => isDead;
        
        /// <summary>
        /// Check if enemy was stomped without resetting the flag (for condition checks)
        /// </summary>
        public bool HasBeenStomped => wasStomped;
        
        /// <summary>
        /// Check if enemy was stomped and reset the flag (for processing)
        /// </summary>
        public bool WasStomped
        {
            get
            {
                bool result = wasStomped;
                wasStomped = false; // Reset after reading
                return result;
            }
        }
        
        /// <summary>
        /// Clear the stomped flag (called after processing death)
        /// </summary>
        public void ClearStompedFlag()
        {
            wasStomped = false;
        }

        private void Awake()
        {
            // Cache hazard components (on self and children)
            hazardComponent = GetComponent<Hazard>();
            hazardComponentsInChildren = GetComponentsInChildren<Hazard>();
            
            // Cache body colliders (exclude head stomp detector collider)
            var allColliders = GetComponents<Collider2D>();
            var headDetectorCollider = headStompDetector != null ? headStompDetector.GetComponent<Collider2D>() : null;
            var bodyColliderList = new System.Collections.Generic.List<Collider2D>();
            foreach (var col in allColliders)
            {
                if (col != headDetectorCollider)
                {
                    bodyColliderList.Add(col);
                }
            }
            bodyColliders = bodyColliderList.ToArray();
            
            // Auto-find head stomp detector if not assigned
            if (headStompDetector == null)
            {
                headStompDetector = GetComponentInChildren<EnemyHeadStompDetector>();
                if (headStompDetector != null)
                {
                    // Re-cache colliders after finding head detector
                    allColliders = GetComponents<Collider2D>();
                    headDetectorCollider = headStompDetector.GetComponent<Collider2D>();
                    bodyColliderList.Clear();
                    foreach (var col in allColliders)
                    {
                        if (col != headDetectorCollider)
                        {
                            bodyColliderList.Add(col);
                        }
                    }
                    bodyColliders = bodyColliderList.ToArray();
                }
                else
                {
                    Debug.LogWarning($"[ENEMY] {gameObject.name} - No head stomp detector found!");
                }
            }

            // Check waypoints
            if (pointA == null)
            {
                Debug.LogWarning($"[ENEMY] {gameObject.name} - Point A is NULL!");
            }
            if (pointB == null)
            {
                Debug.LogWarning($"[ENEMY] {gameObject.name} - Point B is NULL!");
            }
        }

        private void Start()
        {
            // Register with global manager (in Start to ensure manager is initialized)
            RegisterWithManager();
        }

        private void RegisterWithManager()
        {
            if (GlobalEnemyManager.Instance != null)
            {
                GlobalEnemyManager.Instance.RegisterEnemy(this);
            }
            else
            {
                Debug.LogWarning($"[ENEMY] {gameObject.name} - GlobalEnemyManager.Instance is NULL! Searching for manager...");
                
                // Try to find manager if it exists but instance isn't set yet
                var manager = FindFirstObjectByType<GlobalEnemyManager>();
                if (manager != null)
                {
                    manager.RegisterEnemy(this);
                }
                else
                {
                    Debug.LogError($"[ENEMY] {gameObject.name} - NO GlobalEnemyManager found in scene! Enemy will not move!");
                }
            }
        }

        private void OnValidate()
        {
            // Auto-find head stomp detector in editor
            if (headStompDetector == null)
            {
                headStompDetector = GetComponentInChildren<EnemyHeadStompDetector>();
            }
        }

        private void OnDestroy()
        {
            // Unregister from global manager
            GlobalEnemyManager.Instance?.UnregisterEnemy(this);
        }

        /// <summary>
        /// Mark enemy as stomped (called by head stomp detector)
        /// Immediately disables hazard and body colliders to prevent player damage
        /// </summary>
        public void MarkStomped()
        {
            wasStomped = true;
            
            // Immediately disable all hazard components to prevent player damage
            foreach (var hazard in hazardComponentsInChildren)
            {
                if (hazard != null)
                {
                    hazard.enabled = false;
                }
            }
            
            // Remove Hazard tag to prevent collision detection
            if (gameObject.CompareTag("Hazard"))
            {
                gameObject.tag = "Untagged";
            }
            
            // Also remove tag from children that might have it
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Hazard"))
                {
                    child.tag = "Untagged";
                }
            }
            
            // Disable Rigidbody2D physics immediately to prevent blocking player movement
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false;
                rb.velocity = Vector2.zero;
            }
            
            // Disable colliders after a brief delay to allow player to properly detect landing
            // This prevents the jump animation from getting stuck
            if (disableCollidersCoroutine != null)
            {
                StopCoroutine(disableCollidersCoroutine);
            }
            disableCollidersCoroutine = StartCoroutine(DisableCollidersAfterDelay());
        }

        /// <summary>
        /// Disable colliders after a delay to allow player to land properly
        /// </summary>
        private IEnumerator DisableCollidersAfterDelay()
        {
            yield return new WaitForSeconds(colliderDisableDelay);
            
            // Disable ALL colliders (including children) to prevent blocking player movement
            // First disable body colliders on this object
            foreach (var col in bodyColliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
            
            // Also disable all colliders in children (except head stomp detector)
            var allChildColliders = GetComponentsInChildren<Collider2D>();
            int disabledCount = bodyColliders.Length;
            foreach (var col in allChildColliders)
            {
                if (col != null && col != headStompDetector?.GetComponent<Collider2D>())
                {
                    if (col.enabled)
                    {
                        col.enabled = false;
                        disabledCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Mark enemy as dead
        /// </summary>
        public void MarkDead()
        {
            isDead = true;
        }
    }
}

