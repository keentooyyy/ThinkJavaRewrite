using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Climbing")]
    public class StopClimbFlagsAction : ActionTask<Transform>
    {
        public BBParameter<bool> isClimbing;
        public BBParameter<bool> lockToLadderX;

        private Rigidbody2D cachedRigidbody;

        protected override void OnExecute()
        {
            cachedRigidbody = cachedRigidbody ?? (agent ? agent.GetComponent<Rigidbody2D>() : null);

            isClimbing.value = false;
            lockToLadderX.value = false;

            if (cachedRigidbody != null)
            {
                cachedRigidbody.gravityScale = 1f;
                // Unfreeze X but keep rotation frozen
                cachedRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            EndAction(true);
        }
    }
}
