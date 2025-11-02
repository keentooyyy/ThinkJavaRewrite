using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using DG.Tweening;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Climbing")]
    public class ExitLadderTweenAction : ActionTask<Transform>
    {
        public enum ExitSide { Top, Bottom }

        [UnityEngine.Header("Exit Setup")]
        public ExitSide side = ExitSide.Top;
        [Tooltip("Offset from ladder bound: + above top, + below bottom.")]
        public float placeOffset = 0.05f;

        [UnityEngine.Header("Vertical Tween")]
        public float verticalDuration = 0.12f;
        public Ease verticalEase = Ease.OutQuad;

        [UnityEngine.Header("Horizontal Nudge")]
        public bool runSimultaneously = true; // run with Y tween (recommended)
        public float nudgeDistance = 0.08f;
        public float nudgeDuration = 0.12f;
        public Ease nudgeEase = Ease.OutQuad;

        [UnityEngine.Header("Physics Integration")]
        public bool pauseRigidbodyDuringTween = true;

        private Tween tween;
        private Rigidbody2D rb;
        private LadderSensor sensor;

        protected override void OnExecute()
        {
            sensor = agent ? agent.GetComponent<LadderSensor>() : null;
            rb = agent ? agent.GetComponent<Rigidbody2D>() : null;

            bool hadRB = rb != null;
            bool prevSim = true;
            if (hadRB && pauseRigidbodyDuringTween)
            {
                prevSim = rb.simulated;
                rb.velocity = Vector2.zero;
                rb.simulated = false; // prevent physics jitter while tweening
            }

            float boundY;
            if (sensor != null)
            {
                boundY = side == ExitSide.Top
                    ? sensor.LadderTopY() + Mathf.Abs(placeOffset)
                    : sensor.LadderBottomY() - Mathf.Abs(placeOffset);
            }
            else
            {
                boundY = agent.position.y;
            }

            // Facing from localScale.x sign (fallback +1)
            float dir = Mathf.Sign(agent.localScale.x);
            if (Mathf.Approximately(dir, 0f)) dir = 1f;
            float targetX = agent.position.x + dir * Mathf.Abs(nudgeDistance);

            var seq = DOTween.Sequence();
            var yTween = agent.DOMoveY(boundY, verticalDuration).SetEase(verticalEase);
            var xTween = agent.DOMoveX(targetX, Mathf.Max(0.01f, nudgeDuration)).SetEase(nudgeEase);

            if (Mathf.Abs(nudgeDistance) > 0.0001f)
            {
                if (runSimultaneously)
                {
                    seq.Append(yTween);
                    seq.Join(xTween);
                }
                else
                {
                    seq.Append(yTween);
                    seq.Append(xTween);
                }
            }
            else
            {
                seq.Append(yTween);
            }

            seq.OnComplete(() =>
            {
                if (hadRB && pauseRigidbodyDuringTween)
                {
                    rb.simulated = prevSim;
                    rb.gravityScale = 1f;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                }
                EndAction(true);
            });

            tween = seq;
        }

        protected override void OnStop()
        {
            if (tween != null && tween.IsActive())
            {
                tween.Kill(false);
            }

            if (rb != null && pauseRigidbodyDuringTween && !rb.simulated)
            {
                rb.simulated = true;
                rb.gravityScale = 1f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }
}
