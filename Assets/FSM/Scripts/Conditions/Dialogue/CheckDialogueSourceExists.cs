using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("Custom/Dialogue")]
    [Description("Check if nearby dialogue source exists")]
    public class CheckDialogueSourceExists : ConditionTask
    {
        [BlackboardOnly]
        [Tooltip("The nearby dialogue source GameObject to check")]
        public BBParameter<GameObject> nearbyDialogueSource;

        protected override string info => "Dialogue Source Exists?";

        protected override bool OnCheck()
        {
            return nearbyDialogueSource.value != null;
        }
    }
}

