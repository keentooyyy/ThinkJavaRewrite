using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using DG.Tweening;
using GameHazards;

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

