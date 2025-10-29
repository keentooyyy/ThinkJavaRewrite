using NodeCanvas.Framework;
using ParadoxNotion.Design;
using TouchControls;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Wait for jump button press - supports touch events and keyboard/gamepad input")]
    public class WaitForJumpEvent : ConditionTask
    {
        [Tooltip("Keyboard/Gamepad button name from Input Manager (e.g., 'Jump', 'space')")]
        public BBParameter<string> inputButton = "Jump";

        private bool jumpEventReceived = false;

        protected override string info
        {
            get { return "Wait for Jump Press"; }
        }

        protected override void OnEnable()
        {
            TouchInputManager.OnJumpPressed += OnJumpPressed;
            jumpEventReceived = false;
        }

        protected override void OnDisable()
        {
            TouchInputManager.OnJumpPressed -= OnJumpPressed;
        }

        protected override bool OnCheck()
        {
            // Check touch event
            if (jumpEventReceived)
            {
                jumpEventReceived = false;
                return true;
            }

            // Check keyboard/gamepad input
            if (Input.GetButtonDown(inputButton.value))
            {
                return true;
            }

            return false;
        }

        private void OnJumpPressed()
        {
            jumpEventReceived = true;
        }
    }
}

