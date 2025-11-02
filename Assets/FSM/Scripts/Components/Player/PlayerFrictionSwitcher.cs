using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerFrictionSwitcher : MonoBehaviour
{
    [Header("Physics Materials")]
    [SerializeField] private PhysicsMaterial2D groundMaterial;
    [SerializeField] private PhysicsMaterial2D airMaterial;

    private Collider2D cachedCollider;
    private bool currentGrounded;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();

        if (groundMaterial != null)
        {
            cachedCollider.sharedMaterial = groundMaterial;
        }
    }

    public void SetGrounded(bool grounded)
    {
        if (cachedCollider == null || currentGrounded == grounded)
        {
            currentGrounded = grounded;
            return;
        }

        currentGrounded = grounded;

        if (grounded)
        {
            if (groundMaterial != null)
            {
                cachedCollider.sharedMaterial = groundMaterial;
            }
        }
        else if (airMaterial != null)
        {
            cachedCollider.sharedMaterial = airMaterial;
        }
    }
}
