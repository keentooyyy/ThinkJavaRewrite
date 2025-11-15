using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Dialogue;

namespace GameDataBank
{
    /// <summary>
    /// Runtime manager that keeps track of unlocked hints for the current level session.
    /// </summary>
    public class LevelDataBankRuntime : MonoBehaviour
    {
        public static LevelDataBankRuntime Instance { get; private set; }

        [SerializeField]
        private LevelDataBank levelDataBank;

        private readonly Dictionary<string, RuntimeEntry> entriesById = new Dictionary<string, RuntimeEntry>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<DialogueSequence, List<RuntimeEntry>> entriesByDialogue = new Dictionary<DialogueSequence, List<RuntimeEntry>>();
        private readonly List<RuntimeEntry> cachedList = new List<RuntimeEntry>();

        public event Action<RuntimeEntry> EntryUnlocked;

        public bool HasDataBank => levelDataBank != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Multiple LevelDataBankRuntime instances detected. Using instance on '{Instance.gameObject.name}'.");
                enabled = false;
                return;
            }

            Instance = this;
            BuildRuntimeEntries();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void BuildRuntimeEntries()
        {
            entriesById.Clear();
            entriesByDialogue.Clear();
            cachedList.Clear();

            if (levelDataBank == null || levelDataBank.Entries == null)
            {
                return;
            }

            foreach (var definition in levelDataBank.Entries)
            {
                if (definition == null || string.IsNullOrEmpty(definition.EntryId))
                {
                    continue;
                }

                var runtime = new RuntimeEntry(definition);
                entriesById[definition.EntryId] = runtime;
                cachedList.Add(runtime);

                if (definition.UnlockDialogue != null)
                {
                    if (!entriesByDialogue.ContainsKey(definition.UnlockDialogue))
                    {
                        entriesByDialogue[definition.UnlockDialogue] = new List<RuntimeEntry>();
                    }
                    entriesByDialogue[definition.UnlockDialogue].Add(runtime);
                }
            }

            foreach (var entry in cachedList.Where(e => e.Definition.UnlockedByDefault))
            {
                entry.Unlock();
            }
        }

        public IEnumerable<RuntimeEntry> GetAllEntries() => cachedList;

        public IEnumerable<RuntimeEntry> GetUnlockedEntries()
        {
            foreach (var entry in cachedList)
            {
                if (entry.IsUnlocked)
                {
                    yield return entry;
                }
            }
        }

        public void UnlockEntry(string entryId)
        {
            if (string.IsNullOrEmpty(entryId))
            {
                return;
            }

            if (entriesById.TryGetValue(entryId, out var entry))
            {
                Unlock(entry);
            }
        }

        public void UnlockByDialogue(DialogueSequence sequence)
        {
            if (sequence == null)
            {
                return;
            }

            if (entriesByDialogue.TryGetValue(sequence, out var entries))
            {
                foreach (var entry in entries)
                {
                    if (entry != null && entry.Definition.UnlockOnDialogueComplete)
                    {
                        Unlock(entry);
                    }
                }
            }
        }

        private void Unlock(RuntimeEntry entry)
        {
            if (entry == null || entry.IsUnlocked)
            {
                return;
            }

            entry.Unlock();
            EntryUnlocked?.Invoke(entry);
        }

        [Serializable]
        public class RuntimeEntry
        {
            public DataBankEntryDefinition Definition { get; }
            public bool IsUnlocked { get; private set; }
            public DateTime UnlockedAt { get; private set; }

            public RuntimeEntry(DataBankEntryDefinition definition)
            {
                Definition = definition;
                IsUnlocked = definition != null && definition.UnlockedByDefault;
                UnlockedAt = DateTime.MinValue;
            }

            internal void Unlock()
            {
                if (IsUnlocked)
                {
                    return;
                }

                IsUnlocked = true;
                UnlockedAt = DateTime.Now;
            }
        }
    }
}

