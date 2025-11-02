using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("Custom/Climbing")]
    public class ClimbNearCenterCondition : ConditionTask<Transform>
    {
        private LadderSensor cachedSensor;

        protected override bool OnCheck()
        {
            cachedSensor = cachedSensor ?? (agent ? agent.GetComponent<LadderSensor>() : null);
            return cachedSensor != null && cachedSensor.NearLadderCenter(agent);
        }
    }
}
