using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameState;
namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Timing")]
    [Description("Expose the FSM owner's elapsed time and an adjustable max time (e.g., 180s) for countdowns and progress.")]
    public class ReadOwnerTime : ActionTask
    {
        [Tooltip("Max time in seconds (e.g., 180). Adjust this to change the countdown length.")]
        public BBParameter<float> maxTimeSeconds = 180f;

        [Tooltip("Continuously update outputs every frame while the state is active.")]
        public BBParameter<bool> continuous = true;

        [BlackboardOnly]
        [Tooltip("Output: remaining seconds (max - elapsed), clamped to >= 0.")]
        public BBParameter<float> outRemainingSeconds;

        [BlackboardOnly]
        [Tooltip("Output: normalized progress (elapsed / max), clamped 0..1.")]
        public BBParameter<float> outNormalized;

        private float accumulatedElapsed;

        protected override string info => "Read Owner Time";

        protected override void OnExecute()
        {
            accumulatedElapsed = 0f;
            UpdateOutputs();
            if (!continuous.value)
            {
                EndAction(true);
            }
        }

        protected override void OnUpdate()
        {
            if (continuous.value)
            {
                if (GameFreezeManager.AllowsGameplayUpdate)
                {
                    accumulatedElapsed += Time.deltaTime;
                    UpdateOutputs();
                }
            }
        }

        private void UpdateOutputs()
        {
            float max = Mathf.Max(0.0001f, maxTimeSeconds.value);
            float remaining = Mathf.Max(0f, max - accumulatedElapsed);
            float normalized = Mathf.Clamp01(accumulatedElapsed / max);

            if (outRemainingSeconds != null)
            {
                outRemainingSeconds.value = remaining;
            }

            if (outNormalized != null)
            {
                outNormalized.value = normalized;
            }

            if (continuous.value && remaining <= 0f)
            {
                EndAction(true);
            }
        }
    }
}

