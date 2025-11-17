using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameEnemies;
using System.Collections.Generic;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Enemy")]
    [Description("Move all enemies between their waypoints (Point A and Point B)")]
    public class MoveEnemyBetweenWaypoints : ActionTask
    {
        [Tooltip("Distance threshold to consider waypoint reached")]
        public BBParameter<float> reachDistance = 0.1f;

        [Tooltip("Minimum random delay after reaching waypoint (seconds)")]
        public BBParameter<float> minDelay = 0.5f;

        [Tooltip("Maximum random delay after reaching waypoint (seconds)")]
        public BBParameter<float> maxDelay = 2f;

        [Tooltip("Name of idle animation")]
        public BBParameter<string> idleAnimation = "Idle";

        [Tooltip("Name of move animation")]
        public BBParameter<string> moveAnimation = "Move";

        [Tooltip("Crossfade duration for animations")]
        public BBParameter<float> crossfadeTime = 0.2f;

        // Track each enemy's movement state
        private Dictionary<Enemy, EnemyMovementState> enemyStates = new Dictionary<Enemy, EnemyMovementState>();

        private class EnemyMovementState
        {
            public GameObject currentTarget;
            public bool movingToB = true;
            public float delayTimer = 0f;
            public bool isWaiting = false;
            public bool isFacingRight = true;
            public Animator animator;
            public Transform transform;
            public string currentAnimation = "";
        }

        protected override string info
        {
            get { return "Move All Enemies Between Waypoints"; }
        }

        protected override void OnExecute()
        {
            enemyStates.Clear();
            
            // Initialize all enemies
            var manager = GlobalEnemyManager.Instance;
            if (manager == null)
            {
                Debug.LogError("[MOVE_ENEMIES] GlobalEnemyManager.Instance is NULL! Cannot move enemies!");
                EndAction(false);
                return;
            }

            int initializedCount = 0;
            foreach (var enemy in manager.AliveEnemies)
            {
                if (enemy == null)
                {
                    Debug.LogWarning("[MOVE_ENEMIES] Found NULL enemy in AliveEnemies list!");
                    continue;
                }

                if (enemy.PointA == null)
                {
                    Debug.LogWarning($"[MOVE_ENEMIES] Enemy {enemy.gameObject.name} has NULL PointA!");
                    continue;
                }

                if (enemy.PointB == null)
                {
                    Debug.LogWarning($"[MOVE_ENEMIES] Enemy {enemy.gameObject.name} has NULL PointB!");
                    continue;
                }

                var rb = enemy.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    Debug.LogWarning($"[MOVE_ENEMIES] Enemy {enemy.gameObject.name} has NO Rigidbody2D!");
                    continue;
                }

                var animator = enemy.GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogWarning($"[MOVE_ENEMIES] Enemy {enemy.gameObject.name} has NO Animator! Animation will not work.");
                }
                else
                {
                    // Ensure animator is enabled
                    if (!animator.enabled)
                    {
                        animator.enabled = true;
                    }
                }

                // Determine initial facing direction based on PointA to PointB
                bool initialFacingRight = enemy.PointB.transform.position.x > enemy.PointA.transform.position.x;
                
                var state = new EnemyMovementState
                {
                    movingToB = true,
                    currentTarget = enemy.PointB,
                    delayTimer = 0f,
                    isWaiting = false,
                    isFacingRight = initialFacingRight,
                    animator = animator,
                    transform = enemy.transform,
                    currentAnimation = ""
                };
                
                // Set initial sprite facing direction
                if (state.transform != null)
                {
                    Vector3 scale = state.transform.localScale;
                    if (!initialFacingRight && scale.x > 0)
                    {
                        scale.x *= -1;
                    }
                    else if (initialFacingRight && scale.x < 0)
                    {
                        scale.x *= -1;
                    }
                    state.transform.localScale = scale;
                }
                
                // Set initial move animation before first movement
                // This ensures animation is ready before enemy starts moving
                if (state.animator != null)
                {
                    UpdateAnimation(state, true);
                }
                
                enemyStates[enemy] = state;
                initializedCount++;
            }

            if (initializedCount == 0)
            {
                Debug.LogWarning("[MOVE_ENEMIES] No enemies initialized! Check waypoints and Rigidbody2D components!");
            }
        }

        protected override void OnUpdate()
        {
            var manager = GlobalEnemyManager.Instance;
            if (manager == null)
            {
                Debug.LogError("[MOVE_ENEMIES] OnUpdate() - GlobalEnemyManager.Instance is NULL!");
                EndAction(false);
                return;
            }

            // Update all alive enemies
            var aliveEnemies = manager.AliveEnemies;
            var toRemove = new List<Enemy>();
            int activeCount = 0;

            if (enemyStates.Count == 0)
            {
                Debug.LogWarning("[MOVE_ENEMIES] OnUpdate() - enemyStates is EMPTY! No enemies to move!");
            }

            foreach (var kvp in enemyStates)
            {
                var enemy = kvp.Key;
                var state = kvp.Value;

                // Remove if enemy is dead or destroyed
                if (enemy == null)
                {
                    Debug.LogWarning("[MOVE_ENEMIES] Found NULL enemy in enemyStates!");
                    toRemove.Add(enemy);
                    continue;
                }

                if (enemy.IsDead)
                {
                    // Stop animation for dead enemy
                    if (state.animator != null)
                    {
                        state.animator.enabled = false;
                    }
                    toRemove.Add(enemy);
                    continue;
                }

                if (!aliveEnemies.Contains(enemy))
                {
                    Debug.LogWarning($"[MOVE_ENEMIES] Enemy {enemy.gameObject.name} not in aliveEnemies list!");
                    toRemove.Add(enemy);
                    continue;
                }

                // Get enemy components
                var rb = enemy.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    Debug.LogWarning($"[MOVE_ENEMIES] Enemy {enemy.gameObject.name} has NO Rigidbody2D!");
                    toRemove.Add(enemy);
                    continue;
                }

                if (enemy.PointA == null || enemy.PointB == null)
                {
                    Debug.LogWarning($"[MOVE_ENEMIES] Enemy {enemy.gameObject.name} has NULL waypoints!");
                    toRemove.Add(enemy);
                    continue;
                }

                if (state.currentTarget == null)
                {
                    Debug.LogWarning($"[MOVE_ENEMIES] Enemy {enemy.gameObject.name} has NULL currentTarget!");
                    toRemove.Add(enemy);
                    continue;
                }

                // Update target position
                Vector2 targetPosition = state.currentTarget.transform.position;

                // Calculate direction to target
                Vector2 currentPos = rb.position;
                Vector2 direction = (targetPosition - currentPos).normalized;

                // Handle delay timer if waiting
                if (state.isWaiting)
                {
                    state.delayTimer -= Time.deltaTime;
                    if (state.delayTimer <= 0f)
                    {
                        state.isWaiting = false;
                        // Switch target after delay
                        state.movingToB = !state.movingToB;
                        state.currentTarget = state.movingToB ? enemy.PointB : enemy.PointA;
                        // Recalculate target position after switching
                        targetPosition = state.currentTarget.transform.position;
                        // Play move animation FIRST before starting movement
                        UpdateAnimation(state, true);
                        // Continue to movement logic below
                    }
                    else
                    {
                        // Still waiting - play idle animation and stop movement
                        rb.velocity = new Vector2(0, rb.velocity.y);
                        UpdateAnimation(state, false);
                        continue;
                    }
                }

                // Check if reached waypoint (only if not waiting)
                float distanceToTarget = Vector2.Distance(currentPos, targetPosition);
                if (distanceToTarget <= reachDistance.value && !state.isWaiting)
                {
                    // Start random delay
                    float randomDelay = Random.Range(minDelay.value, maxDelay.value);
                    state.delayTimer = randomDelay;
                    state.isWaiting = true;
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    UpdateAnimation(state, false);
                    continue;
                }

                // Update animation FIRST before applying movement
                // This ensures animation plays before enemy starts moving
                UpdateAnimation(state, true);
                
                // Flip sprite based on movement direction
                if (direction.x > 0.01f && !state.isFacingRight)
                {
                    FlipSprite(state);
                }
                else if (direction.x < -0.01f && state.isFacingRight)
                {
                    FlipSprite(state);
                }
                
                // Move towards target (after animation is set)
                float moveX = direction.x * enemy.MoveSpeed;
                rb.velocity = new Vector2(moveX, rb.velocity.y);
                
                activeCount++;
            }


            // Clean up removed enemies
            foreach (var enemy in toRemove)
            {
                enemyStates.Remove(enemy);
            }
        }

        private void FlipSprite(EnemyMovementState state)
        {
            if (state.transform == null) return;
            
            state.isFacingRight = !state.isFacingRight;
            Vector3 scale = state.transform.localScale;
            scale.x *= -1;
            state.transform.localScale = scale;
        }

        private void UpdateAnimation(EnemyMovementState state, bool isMoving)
        {
            if (state.animator == null) return;
            
            // Ensure animator is enabled before playing animations
            if (!state.animator.enabled)
            {
                state.animator.enabled = true;
            }

            string targetAnimation = isMoving ? moveAnimation.value : idleAnimation.value;

            // Only crossfade if animation changed
            if (state.currentAnimation != targetAnimation)
            {
                state.animator.CrossFade(targetAnimation, crossfadeTime.value);
                state.currentAnimation = targetAnimation;
            }
        }

        protected override void OnStop()
        {
            // Stop all enemies
            var manager = GlobalEnemyManager.Instance;
            if (manager != null)
            {
                foreach (var enemy in manager.AliveEnemies)
                {
                    var rb = enemy.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.velocity = new Vector2(0, rb.velocity.y);
                    }
                }
            }

            enemyStates.Clear();
        }
    }
}

