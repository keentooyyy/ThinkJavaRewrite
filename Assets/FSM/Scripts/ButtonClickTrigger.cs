using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.UI;

namespace NodeCanvas.Tasks.Actions
{
    /// <summary>
    /// Listens for a button click and executes the FSM state.
    /// Use this in a state to wait for button press.
    /// </summary>
    [Category("Input (Legacy System)/UI")]
    [Description("Wait for a UI Button to be clicked")]
    public class WaitButtonClick : ActionTask
    {
        [RequiredField]
        [Tooltip("The UI Button to listen to")]
        public BBParameter<Button> button;
        
        private bool wasClicked = false;

        protected override string info
        {
            get { return string.Format("Wait for {0} click", button); }
        }

        protected override string OnInit()
        {
            if (button.value == null)
            {
                return "Button is not assigned!";
            }
            
            return null;
        }

        protected override void OnExecute()
        {
            wasClicked = false;
            button.value.onClick.AddListener(OnButtonClicked);
        }

        protected override void OnUpdate()
        {
            if (wasClicked)
            {
                EndAction(true);
            }
        }

        protected override void OnStop()
        {
            if (button.value != null)
            {
                button.value.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            wasClicked = true;
        }
    }
}

