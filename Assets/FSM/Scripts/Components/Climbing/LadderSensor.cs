using UnityEngine;

[DisallowMultipleComponent]
public class LadderSensor : MonoBehaviour
{
    public string ladderTag = "Climbable";
    public string touchBoundsName = "Ladder Touch Bounds";
    public string climbBoundsName = "Auto Hop Bounds";

    public float centerSnapTolerance = 0.5f;
    public float edgeBuffer = 0.1f;

    public bool isTouchingLadder;
    public Collider2D ladderTouchCollider;
    public Collider2D ladderBoundsCollider;

    private Collider2D playerCollider;

    void Awake()
    {
        playerCollider = GetComponent<Collider2D>();
    }

    public bool NearLadderCenter(Transform player)
    {
        if (!ladderTouchCollider) return false;
        float cx = ladderTouchCollider.bounds.center.x;
        return Mathf.Abs(player.position.x - cx) <= centerSnapTolerance;
    }

    public float LadderCenterX()
    {
        return ladderTouchCollider ? ladderTouchCollider.bounds.center.x : transform.position.x;
    }

    public float LadderTopY()
    {
        if (ladderBoundsCollider) return ladderBoundsCollider.bounds.max.y;
        return ladderTouchCollider ? ladderTouchCollider.bounds.max.y : transform.position.y;
    }

    public float LadderBottomY()
    {
        if (ladderBoundsCollider) return ladderBoundsCollider.bounds.min.y;
        return ladderTouchCollider ? ladderTouchCollider.bounds.min.y : transform.position.y;
    }

    public bool AtTop()
    {
        if (!playerCollider) return false;
        return playerCollider.bounds.max.y >= LadderTopY() - edgeBuffer;
    }

    public bool AtBottom()
    {
        if (!playerCollider) return false;
        return playerCollider.bounds.min.y <= LadderBottomY() + edgeBuffer;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(ladderTag)) return;
        if (other.name == touchBoundsName)
        {
            isTouchingLadder = true;
            ladderTouchCollider = other;
        }
        else if (other.name == climbBoundsName)
        {
            ladderBoundsCollider = other;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(ladderTag)) return;
        if (other == ladderTouchCollider)
        {
            isTouchingLadder = false;
            ladderTouchCollider = null;
        }
        else if (other == ladderBoundsCollider)
        {
            ladderBoundsCollider = null;
        }
    }
}

