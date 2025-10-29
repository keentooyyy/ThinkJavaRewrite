using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Play movement animation based on input (Idle or Move) - only when grounded")]
    public class PlayMovementAnimation : ActionTask<Animator>
    {
        [BlackboardOnly]
        public BBParameter<float> horizontalInput;

        [BlackboardOnly]
        [Tooltip("Is player grounded? Movement animations only play when true")]
        public BBParameter<bool> isGrounded;

        [Tooltip("Name of idle animation")]
        public BBParameter<string> idleAnimation = "Idle";

        [Tooltip("Name of move animation")]
        public BBParameter<string> moveAnimation = "Move";

        [Tooltip("Crossfade duration")]
        public BBParameter<float> crossfadeTime = 0.2f;

        private string currentAnimation = "";

        protected override string info
        {
            get { return "Play Movement Anim (Grounded)"; }
        }

        protected override void OnExecute()
        {
            // Reset state when this action starts
            currentAnimation = "";
        }

        protected override void OnUpdate()
        {
            UpdateAnimation();
        }

        protected override void OnStop()
        {
            // Clear cached animation when stopping
            currentAnimation = "";
        }

        private void UpdateAnimation()
        {
            if (agent == null) return;

            // Only play movement animations when grounded
            // This prevents overriding jump/fall animations
            if (!isGrounded.value)
            {
                return;
            }

            string targetAnimation;

            // Determine which animation to play
            if (Mathf.Abs(horizontalInput.value) > 0.01f)
            {
                targetAnimation = moveAnimation.value;
            }
            else
            {
                targetAnimation = idleAnimation.value;
            }

            // Only crossfade if animation changed
            if (currentAnimation != targetAnimation)
            {
                agent.CrossFade(targetAnimation, crossfadeTime.value);
                currentAnimation = targetAnimation;
            }
        }
    }
}

