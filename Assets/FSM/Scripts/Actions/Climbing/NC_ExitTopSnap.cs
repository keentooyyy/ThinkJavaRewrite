using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("Climbing/Actions")]
public class NC_ExitTopSnap : ActionTask<Transform>
{
    public BBParameter<bool> isClimbing;
    public BBParameter<bool> lockToLadderX;
    public float placeAbove = 0.05f;

    private Rigidbody2D _rb;
    private LadderSensor _sensor;

    protected override void OnExecute()
    {
        _sensor = _sensor ?? (agent ? agent.GetComponent<LadderSensor>() : null);
        _rb = _rb ?? (agent ? agent.GetComponent<Rigidbody2D>() : null);

        float targetY = _sensor != null ? _sensor.LadderTopY() + placeAbove : agent.position.y;
        agent.position = new Vector3(agent.position.x, targetY, agent.position.z);

        isClimbing.value = false;
        lockToLadderX.value = false;

        if (_rb != null)
            _rb.gravityScale = 1f;

        EndAction(true);
    }
}

