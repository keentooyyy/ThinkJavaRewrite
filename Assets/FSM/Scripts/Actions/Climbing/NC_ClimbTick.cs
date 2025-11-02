using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("Climbing/Actions")]
public class NC_ClimbTick : ActionTask<Transform>
{
    public BBParameter<float> climbVertical;
    private LadderSensor _sensor;

    protected override void OnExecute()
    {
        _sensor = _sensor ?? (agent ? agent.GetComponent<LadderSensor>() : null);
    }

    protected override void OnUpdate()
    {
        float v = Input.GetAxisRaw("Vertical");
        if (_sensor != null)
        {
            if (_sensor.AtTop() && v > 0) v = 0f;
            if (_sensor.AtBottom() && v < 0) v = 0f;
        }
        climbVertical.value = v;
    }
}
