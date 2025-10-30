using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Play jump animation and hold it while in air")]
    public class PlayJumpAnimation : ActionTask<Animator>
    {
        [Tooltip("Name of jump animation")]
        public BBParameter<string> jumpAnimation = "Jump";

        [Tooltip("Crossfade duration")]
        public BBParameter<float> crossfadeTime = 0.1f;

        [BlackboardOnly]
        [Tooltip("Is player grounded? Action finishes when player lands")]
        public BBParameter<bool> isGrounded;

        private bool hasLeftGround = false;

        protected override string info
        {
            get { return $"Play '{jumpAnimation}' (Until Grounded)"; }
        }

        protected override void OnExecute()
        {
            if (agent != null)
            {
                agent.CrossFade(jumpAnimation.value, crossfadeTime.value);
            }
            hasLeftGround = false; // Reset the flag
        }

        protected override void OnUpdate()
        {
            // First, wait until player leaves the ground
            if (!hasLeftGround)
            {
                if (!isGrounded.value)
                {
                    hasLeftGround = true;
                }
                return; // Don't finish yet
            }

            // Now wait until player lands
            if (isGrounded.value)
            {
                EndAction(true);
            }
        }
    }
}

