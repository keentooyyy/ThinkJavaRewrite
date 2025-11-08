using UnityEngine;

namespace GameHazards
{
    /// <summary>
    /// Defines damage and knockback properties for a hazard.
    /// Attach to any GameObject with "Hazard" tag.
    /// Each hazard controls its own damage, knockback, and insta-death settings.
    /// </summary>
    public class Hazard : MonoBehaviour
    {
        [Header("Damage Settings")]
        [Tooltip("Damage amount dealt to player (default: 1)")]
        [SerializeField] private int damageAmount = 1;
        
        [Tooltip("If true, kills player instantly regardless of HP and bypasses invincibility")]
        [SerializeField] private bool isInstaDeath = false;

        [Header("Knockback Settings")]
        [Tooltip("Should this hazard apply knockback?")]
        [SerializeField] private bool applyKnockback = true;
        
        [Tooltip("Knockback force (horizontal)")]
        [SerializeField] private float knockbackForce = 5f;
        
        [Tooltip("Knockback upward force (only applied if player is grounded)")]
        [SerializeField] private float knockbackUpForce = 3f;

        [Header("Invincibility Settings")]
        [Tooltip("Bypass invincibility frames? (Insta-death automatically bypasses)")]
        [SerializeField] private bool bypassInvincibility = false;

        public int DamageAmount => damageAmount;
        public bool IsInstaDeath => isInstaDeath;
        public bool ApplyKnockback => applyKnockback;
        public float KnockbackForce => knockbackForce;
        public float KnockbackUpForce => knockbackUpForce;
        public bool BypassInvincibility => bypassInvincibility || isInstaDeath;

        private void OnValidate()
        {
            // Ensure damage is at least 1 (unless insta-death, which sets HP to 0)
            if (!isInstaDeath && damageAmount < 1)
            {
                damageAmount = 1;
            }
            
            // Insta-death should always bypass invincibility
            if (isInstaDeath)
            {
                bypassInvincibility = true;
            }
        }
    }
}

