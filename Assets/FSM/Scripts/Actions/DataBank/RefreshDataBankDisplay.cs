using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameDataBank;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Data Bank")]
    [Description("Refreshes the data bank display by instantiating prefabs for all entries (max 3). Shows Hidden prefab for locked entries and Info Bank Item prefab for unlocked entries.")]
    public class RefreshDataBankDisplay : ActionTask
    {
        [RequiredField]
        [Tooltip("Container Transform where prefabs will be instantiated")]
        public BBParameter<Transform> container;

        [RequiredField]
        [Tooltip("Prefab to show for locked/undiscovered entries")]
        public BBParameter<GameObject> hiddenPrefab;

        [RequiredField]
        [Tooltip("Prefab to show for unlocked/discovered entries")]
        public BBParameter<GameObject> infoBankItemPrefab;

        [Tooltip("Optional: specific runtime reference. If null, will use LevelDataBankRuntime.Instance")]
        public BBParameter<LevelDataBankRuntime> runtime;

        private const int MAX_ENTRIES = 3;

        protected override string info => "Refresh Data Bank Display";

        protected override void OnExecute()
        {
            var rt = runtime.value ?? LevelDataBankRuntime.Instance;
            var cont = container.value;
            var hidden = hiddenPrefab.value;
            var info = infoBankItemPrefab.value;

            if (rt == null)
            {
                Debug.LogWarning("RefreshDataBankDisplay: No LevelDataBankRuntime found.");
                EndAction(false);
                return;
            }

            if (cont == null)
            {
                Debug.LogWarning("RefreshDataBankDisplay: No container Transform assigned.");
                EndAction(false);
                return;
            }

            if (hidden == null || info == null)
            {
                Debug.LogWarning("RefreshDataBankDisplay: Missing prefab references.");
                EndAction(false);
                return;
            }

            ClearContainer(cont);
            var allEntries = rt.GetAllEntries().Take(MAX_ENTRIES).ToList();

            for (int i = 0; i < MAX_ENTRIES; i++)
            {
                GameObject prefabToSpawn = null;
                string summary = null;

                if (i < allEntries.Count)
                {
                    var entry = allEntries[i];
                    if (entry != null && entry.IsUnlocked)
                    {
                        prefabToSpawn = info;
                        summary = entry.Definition?.Summary;
                    }
                    else
                    {
                        prefabToSpawn = hidden;
                    }
                }
                else
                {
                    prefabToSpawn = hidden;
                }

                if (prefabToSpawn == null)
                {
                    continue;
                }

                var instance = UnityEngine.Object.Instantiate(prefabToSpawn, cont);
                
                if (!string.IsNullOrEmpty(summary))
                {
                    DataBankPrefabHelper.SetPrefabText(instance, summary);
                }
            }

            EndAction(true);
        }

        private void ClearContainer(Transform cont)
        {
            for (int i = cont.childCount - 1; i >= 0; i--)
            {
                var child = cont.GetChild(i);
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}

