using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using DG.Tweening;
using GameHazards;
using GameEnemies;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player")]
    [Description("Handle damage detection and response without leaving current state. Reads damage and knockback from Hazard component.")]
    public class HandleDamageOnCollision : ActionTask<Transform>
    {
        [Tooltip("Tag of hazard objects")]
        public BBParameter<string> hazardTag = "Hazard";

        [BlackboardOnly]
        [Tooltip("Current player HP")]
        public BBParameter<int> playerHP;

        [BlackboardOnly]
        [Tooltip("Is player currently invincible?")]
        public BBParameter<bool> isInvincible;

        [BlackboardOnly]
        [Tooltip("Is player on ground? (for knockback logic)")]
        public BBParameter<bool> isGrounded;

        [BlackboardOnly]
        [Tooltip("Duration of invincibility frames")]
        public BBParameter<float> iframeDuration = 1.5f;

        [UnityEngine.Header("Fallback Settings")]
        [Tooltip("Default damage if hazard has no Hazard component (fallback only)")]
        public BBParameter<int> defaultDamageAmount = 1;

        [Tooltip("Blink effect settings")]
        public BBParameter<int> blinkCount = 5;
        public BBParameter<float> minAlpha = 0.3f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private float iframeTimer = 0f;
        private Tween blinkTween;

        protected override string info
        {
            get { return "Handle Damage (Dynamic)"; }
        }

        protected override void OnExecute()
        {
            // Cache components
            rb = agent.GetComponent<Rigidbody2D>();
            spriteRenderer = agent.GetComponent<SpriteRenderer>();
            iframeTimer = 0f;

            // Subscribe to trigger events using NodeCanvas router
            router.onTriggerEnter2D += OnTriggerEnter2D;
        }

        protected override void OnUpdate()
        {
            // Update iframe timer
            if (isInvincible.value)
            {
                iframeTimer += Time.deltaTime;
                if (iframeTimer >= iframeDuration.value)
                {
                    isInvincible.value = false;
                    iframeTimer = 0f;
                }
            }

            // This action never ends - it runs continuously
        }

        private void OnTriggerEnter2D(ParadoxNotion.EventData<Collider2D> eventData)
        {
            Collider2D collision = eventData.value;
            
            // Check if we hit a hazard
            if (!collision.CompareTag(hazardTag.value))
            {
                return;
            }

            // CRITICAL: Check if this is an enemy that's being stomped
            // This prevents damage when player lands on enemy head (stomp detection)
            if (IsStompingEnemy(collision))
            {
                return;
            }

            // Get Hazard component from the collided object (or its parent)
            Hazard hazard = collision.GetComponent<Hazard>();
            if (hazard == null)
            {
                hazard = collision.GetComponentInParent<Hazard>();
            }

            // Check invincibility (unless hazard bypasses it)
            bool canDamage = hazard == null 
                ? !isInvincible.value 
                : (hazard.BypassInvincibility || !isInvincible.value);

            if (canDamage)
            {
                TakeDamage(collision.transform.position, hazard);
            }
        }

        /// <summary>
        /// Check if player is stomping an enemy (above enemy and falling/landing)
        /// This prevents damage when stomping enemies
        /// </summary>
        private bool IsStompingEnemy(Collider2D hazardCollider)
        {
            // Check if this collider belongs to an enemy
            Enemy enemy = hazardCollider.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = hazardCollider.GetComponentInParent<Enemy>();
            }

            if (enemy == null || enemy.IsDead)
            {
                return false; // Not an enemy or already dead
            }

            // Check if enemy has a head stomp detector (indicates it can be stomped)
            if (enemy.HeadStompDetector == null)
            {
                return false; // Enemy can't be stomped
            }

            // Check if player is above the enemy AND directly on top (not from the side)
            float playerY = agent.position.y;
            float enemyY = enemy.transform.position.y;
            float playerX = agent.position.x;
            float enemyX = enemy.transform.position.x;
            
            float verticalDistance = playerY - enemyY;
            float horizontalDistance = Mathf.Abs(playerX - enemyX);
            
            // Player must be significantly above enemy (not just slightly higher)
            // AND horizontally close (directly above, not from the side)
            bool playerIsAbove = verticalDistance > 0.3f; // At least 0.3 units above
            bool isDirectlyAbove = horizontalDistance < 0.8f; // Within 0.8 units horizontally
            
            if (!playerIsAbove || !isDirectlyAbove)
            {
                return false; // Player is not directly above enemy - allow normal damage
            }

            // Check if player is falling or was recently falling (stomping)
            if (rb != null)
            {
                float playerVelocityY = rb.velocity.y;
                
                // Player is stomping if:
                // 1. Currently falling (negative velocity) AND directly above enemy
                // 2. Or velocity is very low/zero AND player is very close vertically (just landed on top)
                // This prevents damage when player lands on enemy head, even if velocity was zeroed by physics
                bool isFalling = playerVelocityY < -0.5f;
                bool justLanded = playerVelocityY <= 0.1f && verticalDistance < 1.0f; // Very close vertically and low velocity = just landed
                
                if (isFalling || justLanded)
                {
                    return true; // Player is stomping this enemy - don't apply damage
                }
            }

            return false;
        }

        private void TakeDamage(Vector3 hazardPosition, Hazard hazard)
        {
            int damage = hazard != null ? hazard.DamageAmount : defaultDamageAmount.value;
            bool isInstaDeath = hazard != null && hazard.IsInstaDeath;
            
            // Insta-death: set HP to 0 immediately
            if (isInstaDeath)
            {
                playerHP.value = 0;
            }
            else
            {
                // Regular damage
                playerHP.value -= damage;
            }

            // Check if dead - if so, skip all damage feedback
            if (playerHP.value <= 0)
            {
                // Dead - stop all movement immediately
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                }
                
                // No blink, no knockback, no iframes for death
                // Just let the FSM transition to Dead state
                return;
            }

            // Still alive - apply damage feedback (skip for insta-death zones)
            if (!isInstaDeath)
            {
                // Apply invincibility frames (unless bypassed)
                if (hazard == null || !hazard.BypassInvincibility)
                {
                    isInvincible.value = true;
                    iframeTimer = 0f;
                }

                // Apply knockback (only if hazard component exists and says so)
                if (rb != null && hazard != null && hazard.ApplyKnockback)
                {
                    Vector2 knockbackDirection = (agent.position - hazardPosition).normalized;
                    knockbackDirection.y = 0; // Keep horizontal only
                    
                    // Use hazard-specific knockback values
                    Vector2 knockback = new Vector2(
                        knockbackDirection.x * hazard.KnockbackForce,
                        isGrounded.value ? hazard.KnockbackUpForce : 0f // No upward force if in air
                    );
                    
                    rb.velocity = knockback;
                }

                // Start blink effect
                StartBlinkEffect();
            }
        }

        private void StartBlinkEffect()
        {
            if (spriteRenderer == null) return;

            // Kill existing tween
            blinkTween?.Kill();

            // Reset alpha
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;

            // Create blink sequence
            float blinkDuration = iframeDuration.value;
            blinkTween = spriteRenderer.DOFade(minAlpha.value, blinkDuration / (blinkCount.value * 2))
                .SetLoops(blinkCount.value * 2, LoopType.Yoyo)
                .SetEase(Ease.Linear)
                .SetUpdate(false)
                .SetAutoKill(true)
                .OnComplete(() => {
                    if (spriteRenderer != null)
                    {
                        Color finalColor = spriteRenderer.color;
                        finalColor.a = 1f;
                        spriteRenderer.color = finalColor;
                    }
                });
        }

        protected override void OnStop()
        {
            // Unsubscribe from events
            router.onTriggerEnter2D -= OnTriggerEnter2D;

            // Clean up tween
            if (blinkTween != null && blinkTween.IsActive())
            {
                blinkTween.Kill();
            }

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;
            }
        }
    }
}

