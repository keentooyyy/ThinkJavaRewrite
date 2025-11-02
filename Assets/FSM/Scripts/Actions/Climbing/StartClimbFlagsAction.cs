using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Climbing")]
    public class StartClimbFlagsAction : ActionTask<Transform>
    {
        public BBParameter<bool> isClimbing;
        public BBParameter<bool> lockToLadderX;
        public BBParameter<float> ladderCenterX;
        public bool controlGravity = true;

        private Rigidbody2D cachedRigidbody;
        private LadderSensor cachedSensor;

        protected override void OnExecute()
        {
            cachedSensor = cachedSensor ?? (agent ? agent.GetComponent<LadderSensor>() : null);
            cachedRigidbody = cachedRigidbody ?? (agent ? agent.GetComponent<Rigidbody2D>() : null);

            if (cachedSensor == null || !cachedSensor.isTouchingLadder)
            {
                EndAction(false);
                return;
            }

            isClimbing.value = true;
            lockToLadderX.value = true;
            ladderCenterX.value = cachedSensor.LadderCenterX();

            if (cachedRigidbody != null)
            {
                if (controlGravity)
                {
                    cachedRigidbody.gravityScale = 0f;
                    cachedRigidbody.velocity = Vector2.zero;
                }

                // Align once and freeze X to avoid jitter during climb
                var pos = cachedRigidbody.position;
                pos.x = ladderCenterX.value;
                cachedRigidbody.position = pos;
                cachedRigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }

            EndAction(true);
        }
    }
}
