using UnityEngine;

namespace GamePlatforms
{
    /// <summary>
    /// Moves a platform between two points (Point A and Point B) with pauses at each point.
    /// Allows the player to ride on the platform by moving the player's position with the platform.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Waypoints")]
        [Tooltip("Point A - starting position")]
        public Transform pointA;
        
        [Tooltip("Point B - end position")]
        public Transform pointB;

        [Header("Movement Settings")]
        [Tooltip("Speed at which the platform moves (units per second)")]
        [Min(0.1f)]
        public float moveSpeed = 3f;

        [Tooltip("Distance threshold to consider waypoint reached")]
        [Min(0.01f)]
        public float reachDistance = 0.1f;

        [Header("Pause Settings")]
        [Tooltip("Time to pause at each waypoint (seconds)")]
        [Min(0f)]
        public float pauseTime = 1f;

        [Header("Player Riding")]
        [Tooltip("Tag of the player GameObject (default: Player)")]
        public string playerTag = "Player";

        [Tooltip("Layer name for detecting player on top (optional - leave empty to use any collision)")]
        public string playerLayerName = "";

        [Tooltip("Offset from platform top to detect player (prevents false positives)")]
        [Min(0f)]
        public float playerDetectionOffset = 0.1f;

        // Private fields
        private Rigidbody2D rb;
        private Vector2 targetPosition;
        private bool movingToB = true;
        private bool isWaiting = false;
        private float waitTimer = 0f;
        private Rigidbody2D playerOnPlatform = null;
        private Vector2 lastPlatformPosition;
        private Collider2D platformCollider;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            platformCollider = GetComponent<Collider2D>();

            // Ensure Rigidbody2D is set to Kinematic for smooth movement
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        private void Start()
        {
            // Validate waypoints
            if (pointA == null || pointB == null)
            {
                Debug.LogError($"MovingPlatform on {gameObject.name}: Point A and Point B must be assigned!");
                enabled = false;
                return;
            }

            // Keep platform at its original position, determine which waypoint is closer
            Vector2 currentPos = rb.position;
            float distToA = Vector2.Distance(currentPos, pointA.position);
            float distToB = Vector2.Distance(currentPos, pointB.position);

            // Start moving toward the closer waypoint
            if (distToA <= distToB)
            {
                targetPosition = pointA.position;
                movingToB = true; // After reaching A, move to B
            }
            else
            {
                targetPosition = pointB.position;
                movingToB = false; // After reaching B, move to A
            }

            lastPlatformPosition = rb.position;
        }

        private void FixedUpdate()
        {
            if (pointA == null || pointB == null)
                return;

            // Handle waiting at waypoints
            if (isWaiting)
            {
                waitTimer -= Time.fixedDeltaTime;
                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    // Switch target
                    movingToB = !movingToB;
                    targetPosition = movingToB ? pointB.position : pointA.position;
                }
                else
                {
                    // Keep platform stationary while waiting
                    rb.velocity = Vector2.zero;
                    return;
                }
            }

            // Move towards target
            Vector2 currentPos = rb.position;
            Vector2 direction = (targetPosition - currentPos).normalized;
            float distanceToTarget = Vector2.Distance(currentPos, targetPosition);

            // Check if reached waypoint
            if (distanceToTarget <= reachDistance)
            {
                // Snap to exact position
                rb.MovePosition(targetPosition);
                rb.velocity = Vector2.zero;

                // Start waiting
                isWaiting = true;
                waitTimer = pauseTime;
            }
            else
            {
                // Move towards target
                Vector2 moveAmount = direction * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(currentPos + moveAmount);
            }

            // Update platform velocity for player riding
            UpdatePlayerRiding();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Check if player is on top of the platform
            if (collision.gameObject.CompareTag(playerTag))
            {
                // Check if player is above the platform (riding on top)
                if (IsPlayerOnTop(collision))
                {
                    Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        AttachPlayer(playerRb);
                    }
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Keep player attached if they're still on top
            if (collision.gameObject.CompareTag(playerTag))
            {
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null && playerOnPlatform == playerRb)
                {
                    if (!IsPlayerOnTop(collision))
                    {
                        // Player is no longer on top, detach
                        DetachPlayer();
                    }
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            // Detach player when they leave the platform
            if (collision.gameObject.CompareTag(playerTag))
            {
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null && playerOnPlatform == playerRb)
                {
                    DetachPlayer();
                }
            }
        }

        private bool IsPlayerOnTop(Collision2D collision)
        {
            if (platformCollider == null)
                return false;

            // Get the contact point
            ContactPoint2D[] contacts = collision.contacts;
            if (contacts.Length == 0)
                return false;

            // Check if the contact point is on the top of the platform
            Bounds platformBounds = platformCollider.bounds;
            float platformTop = platformBounds.max.y;

            foreach (ContactPoint2D contact in contacts)
            {
                // Check if contact is near the top of the platform
                if (contact.point.y >= platformTop - playerDetectionOffset)
                {
                    // Also check if player's center is above the platform
                    float playerBottom = collision.collider.bounds.min.y;
                    if (playerBottom >= platformTop - playerDetectionOffset)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void AttachPlayer(Rigidbody2D playerRb)
        {
            if (playerOnPlatform != null)
                return; // Already have a player attached

            playerOnPlatform = playerRb;
        }

        private void UpdatePlayerRiding()
        {
            if (playerOnPlatform == null)
            {
                // Update last platform position even when no player is on platform
                lastPlatformPosition = rb.position;
                return;
            }

            // Calculate platform movement delta
            Vector2 platformDelta = rb.position - lastPlatformPosition;
            
            // Move player by the same amount the platform moved
            // This ensures the player rides smoothly on the platform
            if (platformDelta.magnitude > 0.001f) // Only move if platform actually moved
            {
                Vector2 newPlayerPosition = playerOnPlatform.position + platformDelta;
                playerOnPlatform.MovePosition(newPlayerPosition);
            }

            // Update last platform position for next frame
            lastPlatformPosition = rb.position;
        }

        private void DetachPlayer()
        {
            if (playerOnPlatform == null)
                return;

            playerOnPlatform = null;
        }

        private void OnDisable()
        {
            // Detach player if platform is disabled
            DetachPlayer();
        }

        private void OnDestroy()
        {
            // Detach player if platform is destroyed
            DetachPlayer();
        }

        // Gizmos for editor visualization
        private void OnDrawGizmosSelected()
        {
            if (pointA != null && pointB != null)
            {
                // Draw line between waypoints
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pointA.position, pointB.position);

                // Draw waypoint markers
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(pointA.position, 0.3f);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(pointB.position, 0.3f);

                // Draw current target
                if (Application.isPlaying)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(targetPosition, 0.2f);
                }
            }
        }
    }
}

