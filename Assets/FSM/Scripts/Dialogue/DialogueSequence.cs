using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    [CreateAssetMenu(menuName = "Dialogue/Sequence", fileName = "DialogueSequence")]
    public class DialogueSequence : ScriptableObject
    {
        [SerializeField]
        private List<DialogueLine> lines = new List<DialogueLine>();

        public IReadOnlyList<DialogueLine> Lines => lines;
        public int LineCount => lines != null ? lines.Count : 0;

        public DialogueLine GetLine(int index)
        {
            if (lines == null || index < 0 || index >= lines.Count)
            {
                return null;
            }
            return lines[index];
        }
    }

    [System.Serializable]
    public class DialogueLine
    {
        [SerializeField, TextArea(2, 5)]
        private string text;

        [Tooltip("When positive overrides the controller's default letters-per-second. Set <= 0 to use global rate.")]
        [SerializeField]
        private float lettersPerSecond = 0f;

        public string Text => text ?? string.Empty;
        public float LettersPerSecond => lettersPerSecond;
    }
}

