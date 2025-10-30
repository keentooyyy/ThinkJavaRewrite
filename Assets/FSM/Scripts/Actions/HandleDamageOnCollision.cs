using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;
using DG.Tweening;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player")]
    [Description("Handle damage detection and response without leaving current state")]
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

        [Tooltip("Damage amount per hit")]
        public BBParameter<int> damageAmount = 1;

        [Tooltip("Knockback force")]
        public BBParameter<float> knockbackForce = 5f;

        [Tooltip("Knockback upward force")]
        public BBParameter<float> knockbackUpForce = 3f;

        [Tooltip("Blink effect settings")]
        public BBParameter<int> blinkCount = 5;
        public BBParameter<float> minAlpha = 0.3f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private float iframeTimer = 0f;
        private Tween blinkTween;

        protected override string info
        {
            get { return "Handle Damage (Continuous)"; }
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
            
            // Check if we hit a hazard and are not invincible
            if (!isInvincible.value && collision.CompareTag(hazardTag.value))
            {
                TakeDamage(collision.transform.position);
            }
        }

        private void TakeDamage(Vector3 hazardPosition)
        {
            // Reduce HP
            playerHP.value -= damageAmount.value;

            // Start iframes
            isInvincible.value = true;
            iframeTimer = 0f;

            // Apply knockback
            if (rb != null)
            {
                Vector2 knockbackDirection = (agent.position - hazardPosition).normalized;
                knockbackDirection.y = 0; // Keep horizontal only
                
                // Use the actual isGrounded blackboard variable (from CheckGroundedAction)
                // Only apply upward knockback force if player is truly grounded
                Vector2 knockback = new Vector2(
                    knockbackDirection.x * knockbackForce.value,
                    isGrounded.value ? knockbackUpForce.value : 0f // No upward force if in air
                );
                
                rb.velocity = knockback;
            }

            // Start blink effect
            StartBlinkEffect();

            // Check if dead
            if (playerHP.value <= 0)
            {
                // Let another transition handle death
                // or trigger death event here
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

