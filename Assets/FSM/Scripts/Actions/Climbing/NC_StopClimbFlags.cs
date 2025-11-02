using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("Climbing/Actions")]
public class NC_StopClimbFlags : ActionTask<Transform>
{
    public BBParameter<bool> isClimbing;
    public BBParameter<bool> lockToLadderX;

    private Rigidbody2D _rb;

    protected override void OnExecute()
    {
        _rb = _rb ?? (agent ? agent.GetComponent<Rigidbody2D>() : null);

        isClimbing.value = false;
        lockToLadderX.value = false;

        if (_rb != null)
        {
            _rb.gravityScale = 1f;
            // Unfreeze X but keep rotation frozen
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        EndAction(true);
    }
}
