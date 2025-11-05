using UnityEngine;
using Dialogue;

namespace DialogueRuntime
{
    /// <summary>
    /// Attach alongside an Interactable to link a DialogueSequence with it.
    /// </summary>
    [RequireComponent(typeof(GameInteraction.Interactable))]
    public class DialogueSource : MonoBehaviour
    {
        [SerializeField] private DialogueSequence sequence;

        public DialogueSequence Sequence => sequence;

        public bool HasDialogue => sequence != null;
    }
}

