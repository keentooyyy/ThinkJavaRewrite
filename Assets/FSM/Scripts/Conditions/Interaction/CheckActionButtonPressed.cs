using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameInput;
using UnityEngine;
using GameInteraction;


namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Interaction")]
    [Description("Check if an action button was pressed this frame. Optionally pull the button name from an Interactable.")]
    public class CheckActionButtonPressed : ConditionTask
    {
        [ParadoxNotion.Design.Header("Interactable Source (optional)")]
        [Tooltip("If assigned and 'Use Interactable Button' is true, the button defined on this Interactable will be used.")]
        [BlackboardOnly]
        public BBParameter<GameObject> interactable;

        [Tooltip("When true, read the requiredButton field from the provided Interactable.")]
        public bool useInteractableButton = false;

        [ParadoxNotion.Design.Header("Button Override")]
        [Tooltip("Fallback or explicit button name to check (e.g. ActionA).")]
        [BlackboardOnly]
        public BBParameter<string> buttonName = "ActionA";

        protected override string info => useInteractableButton
            ? "Interactable button pressed?"
            : $"Button {buttonName} pressed?";

        protected override bool OnCheck()
        {
            string buttonToCheck = null;

            if (useInteractableButton && interactable.value != null)
            {
                var provider = interactable.value.GetComponent<IActionButtonProvider>();
                if (provider != null && !string.IsNullOrEmpty(provider.RequiredButton))
                {
                    buttonToCheck = provider.RequiredButton;
                }
            }

            if (string.IsNullOrEmpty(buttonToCheck))
            {
                buttonToCheck = buttonName.value;
            }

            if (string.IsNullOrEmpty(buttonToCheck))
            {
                return false;
            }

            return InputManager.GetButtonDown(buttonToCheck);
        }
    }
}
