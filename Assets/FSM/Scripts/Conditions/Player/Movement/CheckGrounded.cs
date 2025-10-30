using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

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

            bool grounded = Physics2D.OverlapCircle(
                checkPosition, 
                checkRadius.value, 
                groundLayer
            );

            isGrounded.value = grounded;
            return grounded;
        }
    }
}

