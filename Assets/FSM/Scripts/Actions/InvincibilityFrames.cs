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

        private float elapsedTime = 0f;

        protected override string info
        {
            get { return $"IFrames ({iframeDuration}s)"; }
        }

        protected override void OnExecute()
        {
            elapsedTime = 0f;
            isInvincible.value = true;
        }

        protected override void OnUpdate()
        {
            elapsedTime += Time.deltaTime;
            
            if (elapsedTime >= iframeDuration.value)
            {
                isInvincible.value = false;
                EndAction(true);
            }
        }
    }
}

