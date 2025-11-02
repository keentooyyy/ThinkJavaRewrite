using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using DG.Tweening;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player")]
    [Description("Blink sprite using DOTween Pro during iframes")]
    public class BlinkSpriteEffect : ActionTask<SpriteRenderer>
    {
        public BBParameter<float> blinkDuration = 1.5f;
        public BBParameter<int> blinkCount = 5;
        public BBParameter<float> minAlpha = 0.3f;

        private Tween blinkTween;

        protected override string info
        {
            get { return $"Blink Effect ({blinkCount}x)"; }
        }

        protected override void OnExecute()
        {
            if (agent != null)
            {
                // Kill any existing tween
                blinkTween?.Kill();

                // Reset alpha
                Color color = agent.color;
                color.a = 1f;
                agent.color = color;

                // Create blink sequence using DOTween Pro
                blinkTween = agent.DOFade(minAlpha.value, blinkDuration.value / (blinkCount.value * 2))
                    .SetLoops(blinkCount.value * 2, LoopType.Yoyo)
                    .SetEase(Ease.Linear)
                    .SetUpdate(false)
                    .SetAutoKill(true)
                    .OnComplete(() =>
                    {
                        // Ensure fully visible when done
                        if (agent != null)
                        {
                            Color finalColor = agent.color;
                            finalColor.a = 1f;
                            agent.color = finalColor;
                        }
                        EndAction(true);
                    });
            }
            else
            {
                EndAction(false);
            }
        }

        protected override void OnStop()
        {
            // Clean up tween if action is stopped early
            if (blinkTween != null && blinkTween.IsActive())
            {
                blinkTween.Kill();
            }

            if (agent != null)
            {
                Color color = agent.color;
                color.a = 1f;
                agent.color = color;
            }
        }
    }
}
