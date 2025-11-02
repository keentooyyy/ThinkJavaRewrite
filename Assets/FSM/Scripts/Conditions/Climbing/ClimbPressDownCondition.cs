using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInput;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("Custom/Climbing")]
    public class ClimbPressDownCondition : ConditionTask
    {
        public float deadzone = 0.2f;

        protected override bool OnCheck()
        {
            float vertical = InputManager.GetVerticalAxis();
            if (Mathf.Approximately(vertical, 0f))
            {
                vertical = Input.GetAxisRaw("Vertical");
            }

            return vertical < -deadzone;
        }
    }
}
