using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Dialogue;
using DialogueRuntime;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Dialogue")]
    [Description("Fetch the DialogueSequence attached to a nearby interactable (via DialogueSource component).")]
    public class GetDialogueFromInteractable : ActionTask
    {
        [RequiredField]
        [Tooltip("GameObject reference to the interactable (e.g. from DetectNearbyInteractable).")]
        public BBParameter<GameObject> interactable;

        [BlackboardOnly]
        [Tooltip("Output dialogue sequence reference.")]
        public BBParameter<DialogueSequence> dialogue;

        protected override string info => "Get Dialogue From Interactable";

        protected override void OnExecute()
        {
            dialogue.value = null;

            var target = interactable.value;
            if (target == null)
            {
                EndAction(false);
                return;
            }

            var source = target.GetComponent<DialogueSource>();
            if (source == null || !source.HasDialogue)
            {
                EndAction(false);
                return;
            }

            dialogue.value = source.Sequence;
            EndAction(true);
        }
    }
}

