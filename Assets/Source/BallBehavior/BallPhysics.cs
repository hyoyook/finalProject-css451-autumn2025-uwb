using UnityEngine;

/// <summary>
/// Controls billiard ball physics to make them stop rolling sooner.
/// Attach this to each ball or ball prefab.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BallPhysics : MonoBehaviour
{
    [Header("Drag Settings")]
    [Tooltip("Linear drag - higher values make the ball stop translating sooner (0-10)")]
    [Range(0f, 10f)]
    public float LinearDrag = 2.0f;

    [Tooltip("Angular drag - higher values make the ball stop rotating sooner (0-10)")]
    [Range(0f, 10f)]
    public float AngularDrag = 2.0f;

    [Header("Stopping Threshold")]
    [Tooltip("If velocity drops below this, force the ball to stop completely")]
    public float StopVelocityThreshold = 0.05f;

    [Tooltip("If angular velocity drops below this, force rotation to stop")]
    public float StopAngularVelocityThreshold = 0.05f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Apply drag settings
        rb.linearDamping = LinearDrag;
        rb.angularDamping = AngularDrag;

        Debug.Log($"[BallPhysics] {gameObject.name} initialized with LinearDrag={LinearDrag}, AngularDrag={AngularDrag}");
    }

    private void FixedUpdate()
    {
        // Force stop if velocity is very low (prevents endless slow rolling)
        if (rb.linearVelocity.magnitude < StopVelocityThreshold)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (rb.angularVelocity.magnitude < StopAngularVelocityThreshold)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Update drag values at runtime
    /// </summary>
    public void SetDrag(float linear, float angular)
    {
        LinearDrag = linear;
        AngularDrag = angular;
        
        if (rb != null)
        {
            rb.linearDamping = LinearDrag;
            rb.angularDamping = AngularDrag;
        }
    }

    /// <summary>
    /// Force the ball to stop immediately
    /// </summary>
    public void ForceStop()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
