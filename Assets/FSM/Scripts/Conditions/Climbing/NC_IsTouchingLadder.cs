using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("Climbing/Conditions")]
public class NC_IsTouchingLadder : ConditionTask<Transform>
{
    private LadderSensor _sensor;
    protected override bool OnCheck()
    {
        _sensor = _sensor ?? (agent ? agent.GetComponent<LadderSensor>() : null);
        return _sensor && _sensor.isTouchingLadder;
    }
}

