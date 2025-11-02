using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameScoring;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Scoring")]
    [Description("Compute level score/stars from remaining time stored on the LevelScore blackboard and optionally trigger a UI event.")]
    public class ComputeLevelScore : ActionTask
    {
        [RequiredField]
        [Tooltip("Blackboard int to store computed score.")]
        public BBParameter<int> outScore;

        [Tooltip("Blackboard int to store computed stars (optional).")]
        public BBParameter<int> outStars;

        [Tooltip("Optional UI event to trigger after computing score (e.g., ShowScoreUI)")]
        public BBParameter<string> onComputedEventName;

        protected override string info => "Compute Level Score";

        protected override void OnExecute()
        {
            // Strictly resolve from LevelScore blackboard (remaining or max-elapsed). No elapsed fallback.
            float scoreSeconds = ResolveScoreSeconds();
            int score = LevelScoreCalculator.CalcLevelScore(scoreSeconds);
            int stars = LevelScoreCalculator.CalcLevelStars(scoreSeconds);

            if (outScore != null) outScore.value = score;
            if (outStars != null) outStars.value = stars;

            // Publish to runtime holder so UI can read a single source of truth
            LevelScoreRuntime.LastScore = score;
            LevelScoreRuntime.LastStars = stars;

            var evt = onComputedEventName != null ? onComputedEventName.value : null;
            if (!string.IsNullOrEmpty(evt))
            {
                UIEventManager.Trigger(evt);
            }

            EndAction(true);
        }

        private float ResolveScoreSeconds()
        {
            var bb = blackboard;
            if (bb == null) return 0f;

            var remainingVar = bb.GetVariable<float>("outRemainingSeconds");
            if (remainingVar != null) return Mathf.Max(0f, remainingVar.value);

            var maxVar = bb.GetVariable<float>("maxTime");
            var elapsedVar = bb.GetVariable<float>("outElapsedSeconds");
            if (maxVar != null && elapsedVar != null)
            {
                float maxTime = maxVar.value;
                float elapsed = Mathf.Max(0f, elapsedVar.value);
                if (maxTime > 0f) return Mathf.Max(0f, maxTime - elapsed);
            }

            // No remaining-time source available.
            return 0f;
        }
    }
}
