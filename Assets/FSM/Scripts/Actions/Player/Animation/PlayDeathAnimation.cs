using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player")]
    [Description("Play death animation")]
    public class PlayDeathAnimation : ActionTask<Animator>
    {
        [Tooltip("Name of death animation")]
        public BBParameter<string> deathAnimation = "Death";

        [Tooltip("Crossfade duration")]
        public BBParameter<float> crossfadeTime = 0.1f;

        [Tooltip("How long to wait before finishing (allows animation to play)")]
        public BBParameter<float> deathDuration = 1.5f;

        private float elapsedTime = 0f;

        protected override string info
        {
            get { return $"Play '{deathAnimation}' ({deathDuration}s)"; }
        }

        protected override void OnExecute()
        {
            if (agent != null)
            {
                agent.CrossFade(deathAnimation.value, crossfadeTime.value);
            }
            elapsedTime = 0f;
        }

        protected override void OnUpdate()
        {
            elapsedTime += Time.deltaTime;
            
            if (elapsedTime >= deathDuration.value)
            {
                EndAction(true);
            }
        }
    }
}

