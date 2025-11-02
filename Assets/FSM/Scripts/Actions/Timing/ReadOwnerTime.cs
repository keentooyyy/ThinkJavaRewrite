using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

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

        [RequiredField]
        [BlackboardOnly]
        [Tooltip("Output: elapsed seconds since this FSM started running.")]
        public BBParameter<float> outElapsedSeconds;

        [BlackboardOnly]
        [Tooltip("Output: remaining seconds (max - elapsed), clamped to >= 0.")]
        public BBParameter<float> outRemainingSeconds;

        [BlackboardOnly]
        [Tooltip("Output: normalized progress (elapsed / max), clamped 0..1.")]
        public BBParameter<float> outNormalized;

        protected override string info => "Read Owner Time";

        protected override void OnExecute()
        {
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
                UpdateOutputs();
            }
        }

        private void UpdateOutputs()
        {
            float elapsed = ownerSystemElapsedTime;
            float max = Mathf.Max(0.0001f, maxTimeSeconds.value);
            float remaining = Mathf.Max(0f, max - elapsed);
            float normalized = Mathf.Clamp01(elapsed / max);

            if (outElapsedSeconds != null) outElapsedSeconds.value = elapsed;
            if (outRemainingSeconds != null) outRemainingSeconds.value = remaining;
            if (outNormalized != null) outNormalized.value = normalized;
        }
    }
}

