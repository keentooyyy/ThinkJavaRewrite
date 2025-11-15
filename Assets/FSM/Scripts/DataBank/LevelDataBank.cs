using System;
using System.Collections.Generic;
using UnityEngine;
using Dialogue;

namespace GameDataBank
{
    [CreateAssetMenu(menuName = "Game/Data Bank/Level Data Bank", fileName = "LevelDataBank")]
    public class LevelDataBank : ScriptableObject
    {
        [SerializeField]
        private List<DataBankEntryDefinition> entries = new List<DataBankEntryDefinition>();

        public IReadOnlyList<DataBankEntryDefinition> Entries => entries;

        public DataBankEntryDefinition FindById(string id)
        {
            if (string.IsNullOrEmpty(id) || entries == null)
            {
                return null;
            }

            return entries.Find(e => e != null && string.Equals(e.EntryId, id, StringComparison.OrdinalIgnoreCase));
        }

        public DataBankEntryDefinition FindByDialogue(DialogueSequence sequence)
        {
            if (sequence == null || entries == null)
            {
                return null;
            }

            return entries.Find(e => e != null && e.UnlockDialogue == sequence);
        }
    }

    [Serializable]
    public class DataBankEntryDefinition
    {
        [SerializeField]
        private string entryId = "Hint_001";

        [SerializeField, TextArea(2, 6)]
        private string summary = "Describe the key info the player should remember.";

        [SerializeField]
        private Sprite icon;

        [Header("Unlock Conditions")]
        [SerializeField]
        private DialogueSequence unlockDialogue;

        [SerializeField]
        private bool unlockOnDialogueComplete = true;

        [SerializeField]
        private bool unlockedByDefault = false;

        public string EntryId => entryId;
        public string Summary => summary;
        public Sprite Icon => icon;
        public DialogueSequence UnlockDialogue => unlockDialogue;
        public bool UnlockOnDialogueComplete => unlockOnDialogueComplete;
        public bool UnlockedByDefault => unlockedByDefault;
    }
}

