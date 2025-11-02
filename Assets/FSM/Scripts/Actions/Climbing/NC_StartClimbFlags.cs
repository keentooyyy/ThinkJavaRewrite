using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("Climbing/Actions")]
public class NC_StartClimbFlags : ActionTask<Transform>
{
    public BBParameter<bool> isClimbing;
    public BBParameter<bool> lockToLadderX;
    public BBParameter<float> ladderCenterX;
    public bool controlGravity = true;

    private Rigidbody2D _rb;
    private LadderSensor _sensor;

    protected override void OnExecute()
    {
        _sensor = _sensor ?? (agent ? agent.GetComponent<LadderSensor>() : null);
        _rb = _rb ?? (agent ? agent.GetComponent<Rigidbody2D>() : null);

        if (_sensor == null || !_sensor.isTouchingLadder)
        {
            EndAction(false);
            return;
        }

        isClimbing.value = true;
        lockToLadderX.value = true;
        ladderCenterX.value = _sensor.LadderCenterX();

        if (_rb != null)
        {
            if (controlGravity)
            {
                _rb.gravityScale = 0f;
                _rb.velocity = Vector2.zero;
            }
            // Align once and freeze X to avoid jitter during climb
            var pos = _rb.position;
            pos.x = ladderCenterX.value;
            _rb.position = pos;
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }

        EndAction(true);
    }
}
