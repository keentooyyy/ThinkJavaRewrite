using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Flip player sprite based on movement direction")]
    public class FlipPlayerSprite : ActionTask<Transform>
    {
        [BlackboardOnly]
        public BBParameter<float> horizontalInput;

        [BlackboardOnly]
        public BBParameter<bool> isFacingRight = true;

        protected override string info
        {
            get { return "Flip Sprite"; }
        }

        protected override void OnUpdate()
        {
            if (horizontalInput.value > 0.01f && !isFacingRight.value)
            {
                Flip();
            }
            else if (horizontalInput.value < -0.01f && isFacingRight.value)
            {
                Flip();
            }
        }

        private void Flip()
        {
            isFacingRight.value = !isFacingRight.value;
            Vector3 scale = agent.localScale;
            scale.x *= -1;
            agent.localScale = scale;
        }
    }
}

