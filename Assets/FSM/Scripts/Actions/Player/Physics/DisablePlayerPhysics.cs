using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player")]
    [Description("Disable player physics/movement on death")]
    public class DisablePlayerPhysics : ActionTask<Rigidbody2D>
    {
        [Tooltip("Stop all velocity")]
        public BBParameter<bool> stopVelocity = true;

        [Tooltip("Make kinematic (disable physics)")]
        public BBParameter<bool> makeKinematic = false;

        protected override string info
        {
            get { return "Disable Physics"; }
        }

        protected override void OnExecute()
        {
            if (agent != null)
            {
                if (stopVelocity.value)
                {
                    agent.velocity = Vector2.zero;
                }

                if (makeKinematic.value)
                {
                    agent.isKinematic = true;
                }
            }

            EndAction(true);
        }
    }
}

