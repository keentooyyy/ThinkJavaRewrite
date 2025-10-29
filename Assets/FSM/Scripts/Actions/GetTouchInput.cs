using NodeCanvas.Framework;
using ParadoxNotion.Design;
using TouchControls;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Player Movement")]
    [Description("Get input from touch controls and keyboard/gamepad")]
    public class GetTouchInput : ActionTask
    {
        [BlackboardOnly]
        public BBParameter<float> horizontalInput;

        [BlackboardOnly]
        public BBParameter<bool> jumpPressed;

        [Tooltip("Horizontal axis name from Input Manager (e.g., 'Horizontal')")]
        public BBParameter<string> horizontalAxisName = "Horizontal";

        [Tooltip("Jump button name from Input Manager (e.g., 'Jump')")]
        public BBParameter<string> jumpButtonName = "Jump";

        protected override string info
        {
            get { return "Get Input (Touch + Keyboard)"; }
        }

        protected override void OnExecute()
        {
            // Subscribe to touch jump event
            TouchInputManager.OnJumpPressed += OnJumpPressedHandler;
        }

        protected override void OnUpdate()
        {
            // Get horizontal input from touch OR keyboard/gamepad
            float touchInput = TouchInputManager.HorizontalAxis();
            float keyboardInput = Input.GetAxisRaw(horizontalAxisName.value);
            
            // Use touch if available, otherwise use keyboard
            horizontalInput.value = Mathf.Abs(touchInput) > 0.01f ? touchInput : keyboardInput;

            // Check keyboard/gamepad jump button
            if (Input.GetButtonDown(jumpButtonName.value))
            {
                jumpPressed.value = true;
            }
        }

        protected override void OnStop()
        {
            // Unsubscribe from events when this action stops
            TouchInputManager.OnJumpPressed -= OnJumpPressedHandler;
        }

        private void OnJumpPressedHandler()
        {
            // Set jump flag when touch event fires
            jumpPressed.value = true;
        }
    }
}

