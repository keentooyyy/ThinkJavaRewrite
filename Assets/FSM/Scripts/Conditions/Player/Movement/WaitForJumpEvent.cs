using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameInput;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Wait for jump button press - supports virtual buttons and keyboard input")]
    public class WaitForJumpEvent : ConditionTask
    {
        [Tooltip("Button name to check (e.g., 'Jump')")]
        public BBParameter<string> buttonName = "Jump";

        private bool jumpEventReceived = false;

        protected override string info
        {
            get { return "Wait for Jump Press"; }
        }

        protected override void OnEnable()
        {
            InputManager.OnButtonPressed += OnButtonPressed;
            jumpEventReceived = false;
        }

        protected override void OnDisable()
        {
            InputManager.OnButtonPressed -= OnButtonPressed;
        }

        protected override bool OnCheck()
        {
            // Check button event
            if (jumpEventReceived)
            {
                jumpEventReceived = false;
                return true;
            }

            // Also check directly (fallback)
            if (InputManager.GetButtonDown(buttonName.value))
            {
                return true;
            }

            return false;
        }

        private void OnButtonPressed(string pressedButton)
        {
            if (pressedButton == buttonName.value)
            {
                jumpEventReceived = true;
            }
        }
    }
}

