using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInput;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Input")]
    [Description("Get input from virtual buttons and keyboard - configured via InputConfig asset")]
    public class GetInput : ActionTask
    {
        [UnityEngine.Header("Configuration")]
        [Tooltip("Reference to your InputConfig ScriptableObject asset")]
        public BBParameter<InputConfig> inputConfig;
        
        [UnityEngine.Header("Movement Output")]
        [BlackboardOnly]
        public BBParameter<float> horizontalInput;
        
        [BlackboardOnly]
        public BBParameter<float> verticalInput;
        
        [UnityEngine.Header("Button Outputs")]
        [Tooltip("Add button names you want to check (e.g., 'Jump', 'ActionA', 'ActionB')")]
        public ButtonOutput[] buttonOutputs = new ButtonOutput[]
        {
            new ButtonOutput { buttonName = "Jump" },
            new ButtonOutput { buttonName = "ActionA" },
            new ButtonOutput { buttonName = "ActionB" },
        };
        
        [System.Serializable]
        public class ButtonOutput
        {
            public string buttonName = "Jump";
            [BlackboardOnly]
            public BBParameter<bool> output;
        }
        
        protected override string info
        {
            get { return "Get Input (Configurable)"; }
        }
        
        protected override void OnUpdate()
        {
            // Update keyboard input using config
            if (inputConfig.value != null)
            {
                InputManager.UpdateKeyboardInput(inputConfig.value);
            }
            
            // Get movement axes
            horizontalInput.value = InputManager.GetHorizontalAxis();
            verticalInput.value = InputManager.GetVerticalAxis();
            
            // Check all configured button outputs
            foreach (var buttonOutput in buttonOutputs)
            {
                if (InputManager.GetButtonDown(buttonOutput.buttonName))
                {
                    buttonOutput.output.value = true;
                }
            }
        }
    }
}

