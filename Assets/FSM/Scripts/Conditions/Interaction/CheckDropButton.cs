using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInput;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Interaction")]
    [Description("Check if player pressed the drop button while carrying an object")]
    public class CheckDropButton : ConditionTask
    {
        [BlackboardOnly]
        [Tooltip("The currently carried object (must not be null)")]
        public BBParameter<GameObject> carriedObject;
        
        [Tooltip("Which button drops the object? (e.g., 'ActionA' to use same as pickup, or 'ActionB' for different)")]
        public BBParameter<string> dropButton = "ActionA";
        
        protected override string info
        {
            get { return string.Format("Drop Button ({0}) Pressed?", dropButton); }
        }
        
        protected override bool OnCheck()
        {
            // Check if player is actually carrying something
            if (carriedObject.value == null)
                return false;
            
            // Check if the drop button was pressed this frame
            return InputManager.GetButtonDown(dropButton.value);
        }
    }
}

