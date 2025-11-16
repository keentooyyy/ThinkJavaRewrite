using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameEnemies;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Check if player is grounded using overlap check")]
    public class CheckGrounded : ConditionTask<Transform>
    {
        [Tooltip("Name of the ground check child object (default: 'GroundCheck')")]
        public BBParameter<string> groundCheckName = "GroundCheck";

        [Tooltip("Radius of ground check sphere")]
        public BBParameter<float> checkRadius = 0.2f;
        
        [Tooltip("Vertical offset from player position if no GroundCheck child found")]
        public BBParameter<float> groundCheckOffset = -0.5f;

        [Tooltip("What layers count as ground")]
        public LayerMask groundLayer;

        [BlackboardOnly]
        public BBParameter<bool> isGrounded;

        private Transform groundCheck;

        protected override string info
        {
            get { return "Is Grounded?"; }
        }

        protected override string OnInit()
        {
            groundCheck = agent.Find(groundCheckName.value);
            return null;
        }

        protected override bool OnCheck()
        {
            Vector2 checkPosition;
            
            if (groundCheck != null)
            {
                checkPosition = groundCheck.position;
            }
            else
            {
                checkPosition = new Vector2(agent.position.x, agent.position.y + groundCheckOffset.value);
            }

            Collider2D hit = Physics2D.OverlapCircle(
                checkPosition, 
                checkRadius.value, 
                groundLayer
            );

            // Filter out dead/dying enemies from ground check
            bool grounded = false;
            if (hit != null)
            {
                // Check if this collider belongs to a dead or dying enemy
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null)
                {
                    // Check parent for enemy component
                    enemy = hit.GetComponentInParent<Enemy>();
                }
                
                // If it's an enemy, only count as ground if it's alive (stomped enemies still count until dead)
                if (enemy != null)
                {
                    if (!enemy.IsDead)
                    {
                        grounded = true;
                    }
                    // Enemy is dead - don't count as ground
                }
                else
                {
                    // Not an enemy - count as ground normally
                    grounded = true;
                }
            }

            isGrounded.value = grounded;
            return grounded;
        }
    }
}

