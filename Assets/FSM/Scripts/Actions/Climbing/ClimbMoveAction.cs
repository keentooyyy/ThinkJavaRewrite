using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Climbing")]
    public class ClimbMoveAction : ActionTask<Transform>
    {
        [Tooltip("Vertical climb speed in units/sec")]
        public float climbSpeed = 3f;
        [UnityEngine.Header("Animation (optional)")]
        public bool controlAnimation = true;
        public string climbStateName = "Climb";
        public bool setSpeedParam = true;
        public string verticalSpeedParam = "ClimbSpeed"; // set to empty to skip
        public bool playIdleOnStop = true;
        public string idleStateName = "Idle";

        private Rigidbody2D rb;
        private LadderSensor sensor;
        private bool frozeX;
        private Animator animator;

        protected override void OnExecute()
        {
            rb = agent ? agent.GetComponent<Rigidbody2D>() : null;
            sensor = agent ? agent.GetComponent<LadderSensor>() : null;

            if (rb == null || sensor == null)
            {
                EndAction(false);
                return;
            }

            animator = agent ? agent.GetComponent<Animator>() : null;

            // Align once and freeze X via physics to prevent transform/rigidbody fights
            var p = rb.position;
            p.x = sensor.LadderCenterX();
            rb.position = p;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            frozeX = true;

            if (controlAnimation && animator != null && !string.IsNullOrEmpty(climbStateName))
            {
                animator.Play(climbStateName);
            }
        }

        protected override void OnUpdate()
        {
            if (rb == null || sensor == null)
            {
                return;
            }

            float v = Input.GetAxisRaw("Vertical");

            // Clamp movement at ladder ends
            if (sensor.AtTop() && v > 0f) v = 0f;
            if (sensor.AtBottom() && v < 0f) v = 0f;

            // Apply vertical velocity only; gravity disabled while climbing
            rb.velocity = new Vector2(0f, v * climbSpeed);

            if (controlAnimation && setSpeedParam && animator != null && !string.IsNullOrEmpty(verticalSpeedParam))
            {
                animator.SetFloat(verticalSpeedParam, Mathf.Abs(v));
            }
        }

        protected override void OnStop()
        {
            if (rb != null && frozeX)
            {
                // Leave unfreezing to exit actions; ensure no stray X impulse
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }

            if (controlAnimation && playIdleOnStop && animator != null && !string.IsNullOrEmpty(idleStateName))
            {
                animator.Play(idleStateName);
            }
        }
    }
}
