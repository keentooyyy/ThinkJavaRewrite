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

        [Tooltip("Number of consecutive frames player must be grounded to finish (helps with detection reliability)")]
        [SliderField(1, 5)]
        public BBParameter<int> groundedFrameBuffer = 2;

        private bool hasLeftGround = false;
        private int groundedFrameCount = 0;

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
            groundedFrameCount = 0; // Reset grounded frame counter
        }

        protected override void OnUpdate()
        {
            // First, wait until player leaves the ground
            if (!hasLeftGround)
            {
                if (!isGrounded.value)
                {
                    hasLeftGround = true;
                    groundedFrameCount = 0; // Reset counter when we leave ground
                }
                return; // Don't finish yet
            }

            // Now wait until player lands (with frame buffer for reliability)
            if (isGrounded.value)
            {
                groundedFrameCount++;
                
                // Only finish if we've been grounded for the required number of frames
                if (groundedFrameCount >= groundedFrameBuffer.value)
                {
                    EndAction(true);
                }
            }
            else
            {
                // Reset counter if we're not grounded (player might be bouncing or something)
                groundedFrameCount = 0;
            }
        }
    }
}

