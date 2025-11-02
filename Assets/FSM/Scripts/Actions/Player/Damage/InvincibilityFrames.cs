using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player")]
    [Description("Handle invincibility frames duration")]
    public class InvincibilityFrames : ActionTask
    {
        [BlackboardOnly]
        public BBParameter<bool> isInvincible;

        [BlackboardOnly]
        public BBParameter<float> iframeDuration = 1.5f;

        private float localElapsed = 0f;

        protected override string info
        {
            get { return $"IFrames ({iframeDuration}s)"; }
        }

        protected override void OnExecute()
        {
            localElapsed = 0f;
            isInvincible.value = true;
        }

        protected override void OnUpdate()
        {
            localElapsed += Time.deltaTime;

            if (localElapsed >= iframeDuration.value)
            {
                isInvincible.value = false;
                EndAction(true);
            }
        }
    }
}
