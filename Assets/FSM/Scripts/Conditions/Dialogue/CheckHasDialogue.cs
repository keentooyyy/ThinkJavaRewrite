using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using DialogueRuntime;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("Custom/Dialogue")]
    [Description("Check if nearby interactable has dialogue available")]
    public class CheckHasDialogue : ConditionTask
    {
        [BlackboardOnly]
        [Tooltip("The nearby interactable GameObject to check")]
        public BBParameter<GameObject> nearbyInteractable;

        protected override string info => "Has Dialogue?";

        protected override bool OnCheck()
        {
            if (nearbyInteractable.value == null)
                return false;

            var dialogueSource = nearbyInteractable.value.GetComponent<DialogueSource>();
            return dialogueSource != null && dialogueSource.HasDialogue;
        }
    }
}

