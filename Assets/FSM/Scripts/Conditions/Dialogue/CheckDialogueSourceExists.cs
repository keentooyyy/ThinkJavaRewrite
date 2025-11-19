using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using DialogueRuntime;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("Custom/Dialogue")]
    [Description("Check if nearby dialogue source exists (excludes auto-trigger sources)")]
    public class CheckDialogueSourceExists : ConditionTask
    {
        [BlackboardOnly]
        [Tooltip("The nearby dialogue source GameObject to check")]
        public BBParameter<GameObject> nearbyDialogueSource;

        protected override string info => "Dialogue Source Exists?";

        protected override bool OnCheck()
        {
            if (nearbyDialogueSource.value == null)
                return false;

            // Don't detect dialogue sources with auto trigger enabled
            DialogueSource dialogueSource = nearbyDialogueSource.value.GetComponent<DialogueSource>();
            if (dialogueSource != null && dialogueSource.AutoTrigger)
            {
                return false; // Auto-trigger sources are not detected for manual interaction
            }

            return true;
        }
    }
}

