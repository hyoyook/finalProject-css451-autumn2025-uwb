using UnityEngine;

public class CueTipHit : MonoBehaviour
{
    [Header("Shot Settings")]
    public float forceMultiplier = 10f;
    public float minHitSpeed = 5.0f;
    public string targetTag = "CueBall";

    [Header("Spin & Jump Physics")]
    [Tooltip("Spin strength multiplier (higher = more spin effect)")]
    [Range(0f, 100f)]
    public float spinStrength = 30f;

    [Tooltip("Jump force multiplier when hitting below center")]
    [Range(0f, 5f)]
    public float jumpForceMultiplier = 2.0f;

    [Tooltip("Minimum Y offset (below center) required to trigger jump")]
    [Range(0f, 1f)]
    public float jumpThreshold = 0.3f;

    [Header("Debug Visualization")]
    public bool showContactPoint = true;
    public float debugSphereSize = 0.05f;

    private Vector3 lastPosition;
    private Vector3 currentVelocity;
    private float lastHitTime = 0f;

    void Start()
    {
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 displacement = transform.position - lastPosition;
        currentVelocity = displacement / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < lastHitTime + 0.5f) return;

        if (other.CompareTag(targetTag))
        {
            Rigidbody ballRb = other.GetComponent<Rigidbody>();

            if (ballRb != null)
            {
                lastHitTime = Time.time;
                ApplyRealisticPhysics(ballRb, other.transform);
            }
        }
    }

    /// <summary>
    /// Apply physics based on WHERE the cue tip hits the ball
    /// </summary>
    private void ApplyRealisticPhysics(Rigidbody ballRb, Transform ballTransform)
    {
        // 1. Calculate contact point (where cue tip touches ball surface)
        Vector3 ballCenter = ballTransform.position;
        Vector3 cueTipPosition = transform.position;
        Vector3 toBall = ballCenter - cueTipPosition;
        float ballRadius = ballTransform.localScale.x * 0.5f;

        // Find contact point on ball surface
        Vector3 contactPoint = cueTipPosition + toBall.normalized * Vector3.Distance(cueTipPosition, ballCenter);

        // 2. Calculate vertical offset from ball center (-1 = bottom, 0 = center, +1 = top)
        float verticalOffset = (contactPoint.y - ballCenter.y) / ballRadius;
        verticalOffset = Mathf.Clamp(verticalOffset, -1f, 1f);

        // 3. Get shot power
        float hitSpeed = currentVelocity.magnitude;
        if (hitSpeed < minHitSpeed) hitSpeed = minHitSpeed;

        // 4. Base horizontal direction (keep it flat for now)
        Vector3 horizontalDirection = transform.forward;
        horizontalDirection.y = 0;
        horizontalDirection.Normalize();

        // 5. Calculate force direction with optional jump component
        Vector3 forceDirection = horizontalDirection;
        float totalPower = hitSpeed * forceMultiplier;

        // JUMP SHOT: Only if hit BELOW center AND below threshold
        if (verticalOffset < -jumpThreshold)
        {
            // The lower the hit, the more upward force
            float jumpAmount = Mathf.Abs(verticalOffset + jumpThreshold); // 0 to ~0.7
            float upwardForce = jumpAmount * jumpForceMultiplier;
            
            forceDirection = (horizontalDirection + Vector3.up * upwardForce).normalized;
            
            Debug.Log($"🏀 JUMP SHOT! Offset: {verticalOffset:F2}, Upward: {upwardForce:F2}");
        }

        // 6. Apply force at contact point (creates realistic torque)
        ballRb.AddForceAtPosition(
            forceDirection * totalPower,
            contactPoint,
            ForceMode.Impulse
        );

        // 7. Add explicit spin based on vertical hit location
        ApplySpin(ballRb, horizontalDirection, verticalOffset);

        // 8. Debug visualization
        if (showContactPoint)
        {
            Debug.DrawLine(ballCenter, contactPoint, Color.yellow, 2f);
            Debug.DrawRay(contactPoint, forceDirection * 0.5f, Color.red, 2f);
        }

        Debug.Log($"⚡ HIT! Offset: {verticalOffset:F2}, Power: {totalPower:F1}, Direction: {forceDirection}");
    }

    /// <summary>
    /// Apply spin based on vertical hit location
    /// </summary>
    private void ApplySpin(Rigidbody ballRb, Vector3 horizontalDirection, float verticalOffset)
    {
        // Spin axis is perpendicular to shot direction, parallel to table
        Vector3 spinAxis = Vector3.Cross(horizontalDirection, Vector3.up).normalized;

        // Spin direction based on hit location:
        // - Hit BELOW center (negative offset) = BACKSPIN (negative angular velocity)
        // - Hit ABOVE center (positive offset) = TOPSPIN (positive angular velocity)
        // - Hit CENTER (zero offset) = NO SPIN
        
        float spinAmount = -verticalOffset * spinStrength; // Negative because backspin opposes motion
        
        ballRb.angularVelocity = spinAxis * spinAmount;

        // Debug spin type
        string spinType = "FLAT";
        if (verticalOffset < -0.1f) spinType = "BACKSPIN";
        else if (verticalOffset > 0.1f) spinType = "TOPSPIN";
        
        Debug.Log($"🌀 {spinType} | Offset: {verticalOffset:F2}, Spin: {spinAmount:F1} rad/s");
    }

    private void OnDrawGizmos()
    {
        if (!showContactPoint || !Application.isPlaying) return;

        // Draw cue tip position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, debugSphereSize);
    }
}