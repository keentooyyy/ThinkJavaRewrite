using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Move player horizontally using Rigidbody2D")]
    public class MovePlayer : ActionTask<Rigidbody2D>
    {
        [RequiredField]
        public BBParameter<float> moveSpeed = 5f;

        [Tooltip("Movement multiplier when in air (0.7 = 70% speed)")]
        public BBParameter<float> airControlMultiplier = 0.7f;

        [BlackboardOnly]
        public BBParameter<float> horizontalInput;

        [BlackboardOnly]
        public BBParameter<bool> isGrounded;
        
        [BlackboardOnly]
        [Tooltip("Is the player dead? (optional - if set, movement will be disabled when dead)")]
        public BBParameter<bool> isDead;

        protected override string info
        {
            get { return "Move Player"; }
        }

        protected override void OnUpdate()
        {
            // Don't move if dead
            if (isDead != null && isDead.value)
                return;
            
            float speedMultiplier = isGrounded.value ? 1f : airControlMultiplier.value;
            float moveX = horizontalInput.value * moveSpeed.value * speedMultiplier;
            
            agent.velocity = new Vector2(moveX, agent.velocity.y);
        }
    }
}

