using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Check if player is grounded and store in blackboard (Action version)")]
    public class CheckGroundedAction : ActionTask<Transform>
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
        private PlayerFrictionSwitcher frictionSwitcher;

        protected override string info
        {
            get { return "Check Grounded"; }
        }

        protected override string OnInit()
        {
            // Try to find GroundCheck child
            groundCheck = agent.Find(groundCheckName.value);
            frictionSwitcher = agent.GetComponent<PlayerFrictionSwitcher>();
            return null;
        }

        protected override void OnUpdate()
        {
            Vector2 checkPosition;
            
            if (groundCheck != null)
            {
                // Use GroundCheck position
                checkPosition = groundCheck.position;
            }
            else
            {
                // Use offset from player position
                checkPosition = new Vector2(agent.position.x, agent.position.y + groundCheckOffset.value);
            }

            bool grounded = Physics2D.OverlapCircle(
                checkPosition, 
                checkRadius.value, 
                groundLayer
            );

            isGrounded.value = grounded;

            if (frictionSwitcher != null)
            {
                frictionSwitcher.SetGrounded(grounded);
            }
        }
    }
}
