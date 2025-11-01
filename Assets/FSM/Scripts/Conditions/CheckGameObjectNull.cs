using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Blackboard")]
    [Description("Check whether a blackboard reference is or isn’t null.")]
    public class CheckGameObjectNull : ConditionTask
    {
        [BlackboardOnly]
        public BBParameter<Object> target;

        [Tooltip("True = pass when the variable is null. False = pass when it’s not null.")]
        public bool shouldBeNull = true;

        protected override string info => shouldBeNull
            ? $"{target} == null"
            : $"{target} != null";

        protected override bool OnCheck()
        {
            return shouldBeNull
                ? target.value == null
                : target.value != null;
        }
    }
}


