using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameEnemies;
using System.Collections.Generic;
using DG.Tweening;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Enemy")]
    [Description("Handle death for all stomped enemies (disable physics, play death animation, destroy after delay)")]
    public class EnemyDeath : ActionTask
    {
        [Tooltip("Death animation name (optional)")]
        public BBParameter<string> deathAnimation = "Death";

        [Tooltip("Time before destroying enemy (0 = don't destroy)")]
        public BBParameter<float> destroyDelay = 0.5f;

        [Tooltip("Should disable physics on death?")]
        public BBParameter<bool> disablePhysics = true;

        [Tooltip("Should disable colliders on death?")]
        public BBParameter<bool> disableColliders = true;

        [UnityEngine.Header("Death Animation")]
        [Tooltip("Enable blinking effect for dead enemies")]
        [SerializeField] private bool enableBlinking = true;

        [Tooltip("Blink duration (how long to blink before destroy)")]
        [SerializeField] private float blinkDuration = 0.5f;

        [Tooltip("Number of blinks before destroy")]
        [SerializeField] private int blinkCount = 3;

        [Tooltip("Minimum alpha during blink (0 = fully transparent, 1 = fully visible)")]
        [SerializeField] private float minBlinkAlpha = 0.2f;

        [Tooltip("Time bonus in seconds for each enemy defeated")]
        public BBParameter<float> timeBonusPerEnemy = 10f;

        [Tooltip("Name of the blackboard variable for remaining seconds (default: 'outRemainingSeconds')")]
        public BBParameter<string> remainingSecondsVarName = "outRemainingSeconds";

        private Dictionary<Enemy, float> deathTimers = new Dictionary<Enemy, float>();
        private List<Enemy> enemiesToKill = new List<Enemy>();
        private Dictionary<Enemy, Tween> blinkTweens = new Dictionary<Enemy, Tween>();

        protected override string info
        {
            get { return "Handle Enemy Deaths"; }
        }

        protected override void OnExecute()
        {
            deathTimers.Clear();
            enemiesToKill.Clear();

            var manager = GlobalEnemyManager.Instance;
            if (manager == null)
            {
                Debug.LogError("[ENEMY_DEATH] GlobalEnemyManager.Instance is NULL!");
                EndAction(false);
                return;
            }

            // Find all stomped enemies by checking HasBeenStomped (non-resetting check)
            // Then use WasStomped to read and reset the flag for each one
            var stompedEnemies = new List<Enemy>();
            foreach (var enemy in manager.Enemies)
            {
                if (enemy == null)
                {
                    Debug.LogWarning("[ENEMY_DEATH] Found NULL enemy in list!");
                    continue;
                }

                if (enemy.IsDead)
                {
                    continue;
                }

                // Check if stomped without resetting (using HasBeenStomped)
                if (enemy.HasBeenStomped)
                {
                    // Read and reset the flag by accessing WasStomped
                    bool wasStomped = enemy.WasStomped; // This resets the flag
                    if (wasStomped)
                    {
                        stompedEnemies.Add(enemy);
                    }
                }
            }

            // Process all collected stomped enemies
            foreach (var enemy in stompedEnemies)
            {
                enemiesToKill.Add(enemy);
                deathTimers[enemy] = 0f;
                KillEnemy(enemy);
            }

            // Add time bonus for each enemy defeated
            if (stompedEnemies.Count > 0)
            {
                float timeBonus = timeBonusPerEnemy.value * stompedEnemies.Count;
                AddTimeToRemainingTime(timeBonus);
            }

            if (enemiesToKill.Count == 0)
            {
                EndAction(false);
            }
        }

        private void KillEnemy(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("[ENEMY_DEATH] KillEnemy() called with NULL enemy!");
                return;
            }

            enemy.MarkDead();

            // Cache components
            var rb = enemy.GetComponent<Rigidbody2D>();
            var colliders = enemy.GetComponents<Collider2D>();
            var animator = enemy.GetComponent<Animator>();
            var spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            var headStompDetector = enemy.HeadStompDetector;

            // Stop movement
            if (rb != null && disablePhysics.value)
            {
                rb.velocity = Vector2.zero;
                rb.simulated = false;
            }

            // Disable ALL colliders (including children) to prevent blocking player movement
            if (disableColliders.value)
            {
                // Disable all colliders on the enemy itself
                foreach (var col in colliders)
                {
                    if (col != null)
                        col.enabled = false;
                }
                
                // Also disable all colliders in children (except head stomp detector)
                var allChildColliders = enemy.GetComponentsInChildren<Collider2D>();
                foreach (var col in allChildColliders)
                {
                    if (col != null && col != headStompDetector?.GetComponent<Collider2D>())
                    {
                        col.enabled = false;
                    }
                }
            }

            // Stop animator to prevent movement/jump animations from continuing
            if (animator != null)
            {
                // Disable animator to stop all animations
                animator.enabled = false;
            }

            // Start blinking effect if enabled
            if (enableBlinking && spriteRenderer != null)
            {
                StartBlinkingEffect(enemy, spriteRenderer);
            }
        }

        private void StartBlinkingEffect(Enemy enemy, SpriteRenderer spriteRenderer)
        {
            // Kill any existing blink tween for this enemy
            if (blinkTweens.ContainsKey(enemy) && blinkTweens[enemy] != null && blinkTweens[enemy].IsActive())
            {
                blinkTweens[enemy].Kill();
            }

            // Reset alpha to full
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;

            // Calculate blink interval based on duration and count
            float blinkInterval = blinkDuration / (blinkCount * 2f);

            // Create blinking sequence
            Tween blinkTween = spriteRenderer.DOFade(minBlinkAlpha, blinkInterval)
                .SetLoops(blinkCount * 2, LoopType.Yoyo)
                .SetEase(Ease.Linear)
                .SetUpdate(false)
                .OnComplete(() =>
                {
                    // Ensure fully visible when done (before destroy)
                    if (spriteRenderer != null)
                    {
                        Color finalColor = spriteRenderer.color;
                        finalColor.a = 1f;
                        spriteRenderer.color = finalColor;
                    }
                    // Remove from dictionary
                    if (blinkTweens.ContainsKey(enemy))
                    {
                        blinkTweens.Remove(enemy);
                    }
                });

            blinkTweens[enemy] = blinkTween;
        }

        protected override void OnUpdate()
        {
            if (destroyDelay.value <= 0f)
            {
                EndAction(true);
                return;
            }

            var toRemove = new List<Enemy>();

            // Create a copy of keys to avoid collection modification during iteration
            var enemiesToCheck = new List<Enemy>(deathTimers.Keys);

            foreach (var enemy in enemiesToCheck)
            {
                if (enemy == null)
                {
                    toRemove.Add(enemy);
                    continue;
                }

                if (!deathTimers.ContainsKey(enemy))
                    continue;

                deathTimers[enemy] += Time.deltaTime;
                if (deathTimers[enemy] >= destroyDelay.value)
                {
                    // Kill blink tween before destroying
                    if (blinkTweens.ContainsKey(enemy))
                    {
                        if (blinkTweens[enemy] != null && blinkTweens[enemy].IsActive())
                        {
                            blinkTweens[enemy].Kill();
                        }
                        blinkTweens.Remove(enemy);
                    }

                    UnityEngine.Object.Destroy(enemy.gameObject);
                    toRemove.Add(enemy);
                }
            }

            // Remove destroyed enemies from dictionaries
            foreach (var enemy in toRemove)
            {
                deathTimers.Remove(enemy);
            }

            // Finish when all enemies are destroyed
            if (deathTimers.Count == 0)
            {
                EndAction(true);
            }
        }

        private void AddTimeToRemainingTime(float timeToAdd)
        {
            if (timeToAdd <= 0f)
                return;

            // Since EnemyDeath and ReadOwnerTime are on different FSM owners, we need to:
            // 1. Find the GraphOwner that has the remaining time variable
            // 2. Find the ReadOwnerTime action that belongs to that GraphOwner's graph
            
            // First, find the GraphOwner with the variable
            GraphOwner targetGraphOwner = null;
            var allGraphOwners = Object.FindObjectsByType<GraphOwner>(FindObjectsSortMode.None);
            
            foreach (var graphOwner in allGraphOwners)
            {
                if (graphOwner == null || graphOwner.blackboard == null)
                    continue;

                var remainingVar = graphOwner.blackboard.GetVariable<float>(remainingSecondsVarName.value);
                if (remainingVar != null)
                {
                    targetGraphOwner = graphOwner;
                    break;
                }
            }

            if (targetGraphOwner == null)
            {
                Debug.LogWarning($"[ENEMY_DEATH] Could not find remaining time variable '{remainingSecondsVarName.value}' in any GraphOwner blackboard! Make sure ReadOwnerTime is running and the variable name matches.");
                return;
            }

            // Now find the ReadOwnerTime action that belongs to this GraphOwner's graph
            if (targetGraphOwner.graph != null)
            {
                var readOwnerTimeActions = targetGraphOwner.graph.GetAllTasksOfType<ReadOwnerTime>();
                
                foreach (var readOwnerTime in readOwnerTimeActions)
                {
                    if (readOwnerTime == null)
                        continue;

                    // Found the ReadOwnerTime in this graph! Add time by reducing elapsed time
                    readOwnerTime.AddTimeBonus(timeToAdd);
                    return;
                }
            }

            // Fallback: If ReadOwnerTime isn't found, directly modify the variable
            // This will work temporarily but will be overwritten on the next frame
            var fallbackRemainingVar = targetGraphOwner.blackboard.GetVariable<float>(remainingSecondsVarName.value);
            if (fallbackRemainingVar != null)
            {
                float newTime = Mathf.Clamp(fallbackRemainingVar.value + timeToAdd, 0f, 180f);
                fallbackRemainingVar.value = newTime;
                Debug.LogWarning($"[ENEMY_DEATH] ReadOwnerTime action not found on target GraphOwner. Using fallback method (time may be overwritten). Make sure ReadOwnerTime is running on the same GraphOwner that has the '{remainingSecondsVarName.value}' variable.");
            }
        }

        protected override void OnStop()
        {
            // Kill all active blink tweens
            foreach (var kvp in blinkTweens)
            {
                if (kvp.Value != null && kvp.Value.IsActive())
                {
                    kvp.Value.Kill();
                }
            }
            blinkTweens.Clear();

            deathTimers.Clear();
            enemiesToKill.Clear();
        }
    }
}

