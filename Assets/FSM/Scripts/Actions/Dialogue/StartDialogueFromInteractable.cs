using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using DialogueRuntime;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Dialogue")]
    [Description("Gets dialogue from interactable and starts it immediately")]
    public class StartDialogueFromInteractable : ActionTask
    {
        [RequiredField]
        [Tooltip("The interactable GameObject to get dialogue from")]
        public BBParameter<GameObject> interactable;

        [Tooltip("Wait for the dialogue to finish before continuing the FSM.")]
        public bool waitForCompletion = true;

        private bool awaitingCompletion;

        protected override string info => "Start Dialogue From Interactable";

        protected override void OnExecute()
        {
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

            var system = DialogueSystem.Instance;
            if (system == null)
            {
                EndAction(false);
                return;
            }

            if (waitForCompletion)
            {
                awaitingCompletion = true;
                system.BeginDialogue(source.Sequence, OnDialogueFinished);
            }
            else
            {
                system.BeginDialogue(source.Sequence);
                EndAction(true);
            }
        }

        protected override void OnStop()
        {
            if (!awaitingCompletion)
                return;

            var system = DialogueSystem.Instance;
            if (system != null && system.IsActive)
            {
                system.CancelDialogue();
            }

            awaitingCompletion = false;
        }

        private void OnDialogueFinished()
        {
            awaitingCompletion = false;
            EndAction(true);
        }
    }
}

