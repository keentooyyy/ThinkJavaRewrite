using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInput;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Climbing")]
    public class LadderTopGuardAction : ActionTask<Transform>
    {
        [Tooltip("Extra Y offset above ladder top to keep the player from dipping when walking into the top.")]
        public float holdAbove = 0.02f;

        private Rigidbody2D rb;
        private LadderSensor sensor;

        protected override void OnExecute()
        {
            rb = agent ? agent.GetComponent<Rigidbody2D>() : null;
            sensor = agent ? agent.GetComponent<LadderSensor>() : null;
        }

        protected override void OnUpdate()
        {
            if (rb == null || sensor == null)
            {
                return;
            }

            // Only guard when overlapping the ladder top, centered, and not pressing down
            bool touching = sensor.isTouchingLadder;
            bool nearCenter = sensor.NearLadderCenter(agent);
            bool atTop = sensor.AtTop();

            float vertical = InputManager.GetVerticalAxis();
            if (Mathf.Approximately(vertical, 0f))
            {
                vertical = Input.GetAxisRaw("Vertical");
            }
            bool pressingDown = vertical < -0.2f;

            if (touching && nearCenter && atTop && !pressingDown)
            {
                float minY = sensor.LadderTopY() + holdAbove;
                if (agent.position.y < minY)
                {
                    agent.position = new Vector3(agent.position.x, minY, agent.position.z);
                }

                // If downward velocity, cancel it to avoid slipping in
                if (rb.velocity.y < 0f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                }
            }
        }
    }
}
