using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Dialogue;
using DialogueRuntime;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("Custom/Dialogue")]
    [Description("Starts a dialogue sequence via the DialogueSystem. Optionally waits until the dialogue finishes.")]
    public class StartDialogueAction : ActionTask
    {
        [RequiredField]
        [Tooltip("The dialogue sequence to play.")]
        public BBParameter<DialogueSequence> dialogue;

        [Tooltip("Wait for the dialogue to finish before continuing the FSM.")]
        public bool waitForCompletion = true;

        private bool awaitingCompletion;

        protected override string info
        {
            get
            {
                return dialogue != null && dialogue.value != null
                    ? $"Start Dialogue\n\"{dialogue.value.name}\""
                    : "Start Dialogue";
            }
        }

        protected override void OnExecute()
        {
            var system = DialogueSystem.Instance;
            var asset = dialogue.value;

            if (system == null || asset == null)
            {
                EndAction(false);
                return;
            }

            if (waitForCompletion)
            {
                awaitingCompletion = true;
                system.BeginDialogue(asset, OnDialogueFinished);
            }
            else
            {
                system.BeginDialogue(asset);
                EndAction(true);
            }
        }

        protected override void OnStop()
        {
            if (!awaitingCompletion)
            {
                return;
            }

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

