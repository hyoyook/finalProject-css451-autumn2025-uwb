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

    [Header("Gravity Settings")]
    [Tooltip("Multiplier for gravity (1 = normal, 2 = double gravity, 0.5 = half gravity)")]
    [Range(0f, 5f)]
    public float GravityMultiplier = 1.0f;

    [Header("Advanced Spin Physics (Optional)")]
    [Tooltip("Enable realistic spin-to-velocity conversion (makes backspin more effective)")]
    public bool EnableAdvancedSpinPhysics = false;

    [Tooltip("How much spin affects linear velocity (0 = none, higher = more effect)")]
    [Range(0f, 1f)]
    public float SpinFrictionFactor = 0.3f;

    [Tooltip("Y position below which ball is considered on table")]
    public float TableHeight = 2.0f;

    [Tooltip("Distance threshold to check if ball is on table")]
    public float TableCheckDistance = 0.2f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Apply drag settings
        rb.linearDamping = LinearDrag;
        rb.angularDamping = AngularDrag;

        // Debug.Log($"[BallPhysics] {gameObject.name} initialized with LinearDrag={LinearDrag}, AngularDrag={AngularDrag}");
    }

    /// <summary>
    /// Called when values are changed in the Inspector (Editor and Runtime)
    /// </summary>
    private void OnValidate()
    {
        // Update rigidbody if it exists (works in play mode)
        if (rb != null)
        {
            rb.linearDamping = LinearDrag;
            rb.angularDamping = AngularDrag;
        }
    }

    private void FixedUpdate()
    {
        // Apply custom gravity if multiplier is not 1
        if (GravityMultiplier != 1.0f && rb != null)
        {
            // Add extra gravity force (multiplier - 1 because Unity already applies 1x gravity)
            rb.AddForce(Physics.gravity * (GravityMultiplier - 1.0f), ForceMode.Acceleration);
        }

        // Advanced spin physics: Convert spin to linear velocity change (realistic backspin)
        if (EnableAdvancedSpinPhysics && rb != null && IsOnTable())
        {
            if (rb.angularVelocity.magnitude > 0.1f && rb.linearVelocity.magnitude > 0.05f)
            {
                // Calculate tangential velocity at contact point (where ball touches table)
                Vector3 tangentialVelocity = Vector3.Cross(rb.angularVelocity, Vector3.down);
                
                // Apply friction force that converts spin to linear velocity
                // This makes backspin slow the ball down (and eventually reverse it)
                rb.AddForce(-tangentialVelocity * SpinFrictionFactor, ForceMode.Acceleration);
            }
        }

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
    /// Check if ball is on the table surface
    /// </summary>
    private bool IsOnTable()
    {
        if (rb == null) return false;
        
        // Simple check: is ball close to table height?
        return Mathf.Abs(transform.position.y - TableHeight) < TableCheckDistance;
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
    /// Set the gravity multiplier at runtime
    /// </summary>
    public void SetGravityMultiplier(float multiplier)
    {
        GravityMultiplier = Mathf.Clamp(multiplier, 0f, 5f);
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
