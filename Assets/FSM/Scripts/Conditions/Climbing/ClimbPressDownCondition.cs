using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("Custom/Climbing")]
    public class ClimbPressDownCondition : ConditionTask
    {
        public float deadzone = 0.2f;

        protected override bool OnCheck()
        {
            return Input.GetAxisRaw("Vertical") < -deadzone;
        }
    }
}
