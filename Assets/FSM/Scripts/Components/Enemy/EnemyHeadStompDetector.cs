using UnityEngine;
using ParadoxNotion;

namespace GameEnemies
{
    /// <summary>
    /// Detects when player jumps on enemy's head (Mario-style stomp).
    /// Attach to the head collider (capsule collider) of the enemy.
    /// The head collider should be a trigger.
    /// </summary>
    public class EnemyHeadStompDetector : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Tag of the player GameObject")]
        [SerializeField] private string playerTag = "Player";
        
        [Tooltip("Minimum downward velocity required for stomp (negative Y velocity)")]
        [SerializeField] private float minStompVelocity = -2f;

        [Tooltip("Event name to trigger when stomped (will be sent to parent FSM)")]
        [SerializeField] private string stompEventName = "OnStomped";

        private bool wasStomped = false;
        private Enemy cachedEnemy;
        private Collider2D headStompCollider; // Cache the head stomp collider (CapsuleCollider2D)
        private float lastPlayerVelocityY = 0f; // Track player's velocity from previous frame

        private void Awake()
        {
            // Cache enemy reference on awake (works after scene reload)
            cachedEnemy = GetComponentInParent<Enemy>();
            
            // Find the CapsuleCollider2D (head stomp detector) - ignore BoxCollider2D (hazard)
            var allColliders = GetComponents<Collider2D>();
            foreach (var col in allColliders)
            {
                if (col is CapsuleCollider2D)
                {
                    headStompCollider = col;
                    break;
                }
            }
        }

        private void OnEnable()
        {
            // Refresh head stomp collider cache if needed
            if (headStompCollider == null)
            {
                var allColliders = GetComponents<Collider2D>();
                foreach (var col in allColliders)
                {
                    if (col is CapsuleCollider2D)
                    {
                        headStompCollider = col;
                        break;
                    }
                }
            }
            
            // Refresh enemy cache if needed
            if (cachedEnemy == null)
            {
                cachedEnemy = GetComponentInParent<Enemy>();
            }
        }

        /// <summary>
        /// Check if enemy was stomped this frame (resets after being read)
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Check if it's the player
            if (!collision.CompareTag(playerTag))
            {
                return;
            }

            // Use cached enemy reference (refresh if null, e.g., after scene reload)
            if (cachedEnemy == null)
            {
                cachedEnemy = GetComponentInParent<Enemy>();
            }

            if (cachedEnemy == null)
            {
                return;
            }

            // Don't allow stomping if enemy is already dead
            if (cachedEnemy.IsDead)
            {
                return;
            }

            // Check if player is moving downward (jumping on head)
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb == null)
            {
                return;
            }

            float playerVelocityY = playerRb.velocity.y;

            // Check if player is above enemy head (fallback for when velocity is already 0)
            float playerY = collision.transform.position.y;
            float headY = transform.position.y;
            bool playerIsAbove = playerY > headY;
            float distanceToHead = Mathf.Abs(playerY - headY);

            // Stomp if: (velocity is fast enough) OR (player was falling and is now on top)
            // Position check now requires player to have been falling (negative velocity) to prevent standing/walking on enemy from triggering stomp
            bool velocityCheck = playerVelocityY < minStompVelocity;
            bool positionCheck = playerIsAbove && distanceToHead < 1.5f && playerVelocityY < -0.5f; // Must be falling, even if slowly
            
            // Store last velocity for potential use in OnTriggerStay
            lastPlayerVelocityY = playerVelocityY;
            
            if (velocityCheck || positionCheck)
            {
                wasStomped = true;
                
                // Notify parent Enemy component (this is the important part)
                cachedEnemy.MarkStomped();
                
                // Try to trigger event on global enemy manager FSM (optional, won't break if it fails)
                var globalManager = GlobalEnemyManager.Instance;
                if (globalManager != null)
                {
                    try
                    {
                        if (globalManager.graph != null && globalManager.graph.isRunning)
                        {
                            var router = globalManager.gameObject.GetAddComponent<ParadoxNotion.Services.EventRouter>();
                            if (router != null)
                            {
                                router.InvokeCustomEvent(stompEventName, null, this);
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            // Also check in Stay in case player is already in trigger when velocity becomes 0
            // This handles cases where player lands on enemy head but velocity resets before detection
            if (!collision.CompareTag(playerTag))
                return;

            if (cachedEnemy == null)
            {
                cachedEnemy = GetComponentInParent<Enemy>();
            }

            if (cachedEnemy == null || cachedEnemy.IsDead)
                return;

            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb == null)
                return;

            // Check if player is above enemy (player Y > enemy head Y) and moving down
            // Only trigger if player is actively falling, NOT if standing/walking on top
            float playerY = collision.transform.position.y;
            float headY = transform.position.y;
            bool playerIsAbove = playerY > headY;
            
            float playerVelocityY = playerRb.velocity.y;
            
            // Only trigger if player is falling (negative velocity) OR was falling in previous frame
            // This prevents standing/walking on enemy from triggering a stomp
            if (playerIsAbove && (playerVelocityY < -0.5f || lastPlayerVelocityY < -0.5f))
            {
                // Additional check: make sure player is actually on top (not just passing through)
                // Check if player's bottom is near the head's top
                float distanceToHead = Mathf.Abs(playerY - headY);
                if (distanceToHead < 1.5f) // Player is close to head (likely landing on it)
                {
                    if (!wasStomped) // Only process once
                    {
                        wasStomped = true;
                        cachedEnemy.MarkStomped();
                        
                        // Try to trigger event
                        var globalManager = GlobalEnemyManager.Instance;
                        if (globalManager != null)
                        {
                            try
                            {
                                if (globalManager.graph != null && globalManager.graph.isRunning)
                                {
                                    var router = globalManager.gameObject.GetAddComponent<ParadoxNotion.Services.EventRouter>();
                                    if (router != null)
                                    {
                                        router.InvokeCustomEvent(stompEventName, null, this);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            
            // Update last velocity for next frame
            lastPlayerVelocityY = playerVelocityY;
        }
    }
}

