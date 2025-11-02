using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInput;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("Custom/Climbing")]
    public class ClimbPressUpCondition : ConditionTask
    {
        public float deadzone = 0.2f;

        protected override bool OnCheck()
        {
            // Prefer virtual input when available, fall back to hardware axis
            float vertical = InputManager.GetVerticalAxis();
            if (Mathf.Approximately(vertical, 0f))
            {
                vertical = Input.GetAxisRaw("Vertical");
            }

            return vertical > deadzone;
        }
    }
}
