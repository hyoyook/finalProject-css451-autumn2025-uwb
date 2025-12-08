using UnityEngine;

public class CueTipHit : MonoBehaviour
{
    [Header("Shot Settings")]
    public float forceMultiplier = 10f;
    public float minHitSpeed = 5.0f;
    public string targetTag = "Selectable";

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

    [Tooltip("Vertical offset threshold for flat shot (center zone size)")]
    [Range(0f, 0.5f)]
    public float flatShotThreshold = 0.25f;

    [Header("Debug Visualization")]
    public bool showContactPoint = true;
    public float debugSphereSize = 0.05f;

    [Header("References")]
    [Tooltip("Reference to CueStickController (will auto-find if not set)")]
    public CueStickController cueController;

    private Vector3 lastPosition;
    private Vector3 currentVelocity;
    private float lastHitTime = 0f;

    void Start()
    {
        lastPosition = transform.position;
        
        // Debug.Log($"[CueTipHit] Component started on GameObject: {gameObject.name}");

        // Check if we have a collider and if it's a trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Debug.LogError($"[CueTipHit] NO COLLIDER found on {gameObject.name}! OnTriggerEnter will never fire!");
        }
        else
        {
            // Debug.Log($"[CueTipHit] Collider found: {col.GetType().Name}, IsTrigger: {col.isTrigger}, Enabled: {col.enabled}");
            if (!col.isTrigger)
            {
                // Debug.LogWarning($"[CueTipHit] Collider is NOT a trigger! Set 'Is Trigger' to true in Inspector!");
            }
        }

        // CRITICAL: Check for Rigidbody requirement
        Rigidbody rb = GetComponent<Rigidbody>();
        Rigidbody parentRb = GetComponentInParent<Rigidbody>();
        if (rb == null && parentRb == null)
        {
            // Debug.LogWarning($"[CueTipHit] NO RIGIDBODY found on {gameObject.name} or parent! OnTriggerEnter requires at least one object to have a Rigidbody. The cue ball should have one.");
        }
        else
        {
            // Debug.Log($"[CueTipHit] Rigidbody check: Local={rb != null}, Parent={parentRb != null}");
        }

        // Auto-find CueStickController if not assigned
        if (cueController == null)
        {
            cueController = FindObjectOfType<CueStickController>();
            if (cueController == null)
            {
                // Debug.LogWarning("[CueTipHit] Could not find CueStickController. Cue stick won't hide after hit.");
            }
            else
            {
                // Debug.Log($"[CueTipHit] Found CueStickController: {cueController.name}");
            }
        }
        
        // Find and check the cue ball
        GameObject[] possibleBalls = GameObject.FindGameObjectsWithTag(targetTag);
        // Debug.Log($"[CueTipHit] Found {possibleBalls.Length} GameObjects with tag '{targetTag}'");
        foreach (var ball in possibleBalls)
        {
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            Collider ballCol = ball.GetComponent<Collider>();
            // Debug.Log($"[CueTipHit] Ball: {ball.name}, HasRigidbody: {ballRb != null}, HasCollider: {ballCol != null}, ColliderEnabled: {(ballCol != null ? ballCol.enabled.ToString() : "N/A")}");
        }
    }

    void FixedUpdate()
    {
        Vector3 displacement = transform.position - lastPosition;
        currentVelocity = displacement / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log($"[CueTipHit] OnTriggerEnter called! Collided with: {other.gameObject.name}, Tag: {other.tag}");
        
        if (Time.time < lastHitTime + 0.5f)
        {
            // Debug.Log($"[CueTipHit] Too soon since last hit, ignoring");
            return;
        }

        if (other.CompareTag(targetTag))
        {
            // Debug.Log($"[CueTipHit] Tag matches '{targetTag}'! Applying physics...");
            Rigidbody ballRb = other.GetComponent<Rigidbody>();

            if (ballRb != null)
            {
                lastHitTime = Time.time;
                ApplyRealisticPhysics(ballRb, other.transform);
            }
            else
            {
                // Debug.LogWarning($"[CueTipHit] Ball has no Rigidbody!");
            }
        }
        else
        {
            // Debug.Log($"[CueTipHit] Tag '{other.tag}' doesn't match target tag '{targetTag}'");
        }
    }

    /// <summary>
    /// Apply physics based on WHERE the cue tip hits the ball
    /// </summary>
    private void ApplyRealisticPhysics(Rigidbody ballRb, Transform ballTransform)
    {
        // 1. Get the cue stick's forward direction and tip position
        Vector3 cueForward = -transform.right;
        Vector3 cueTipPos = transform.position;
        
        // 2. Calculate ball parameters
        Vector3 ballCenter = ballTransform.position;
        float ballRadius = ballTransform.localScale.x * 0.5f;

        // 3. Simple and reliable: Use the cue tip's Y position relative to ball center
        // This is much more stable than complex ray-sphere intersection
        float verticalOffset = (cueTipPos.y - ballCenter.y) / ballRadius;
        verticalOffset = Mathf.Clamp(verticalOffset, -1f, 1f);
        
        // 4. Calculate contact point on ball surface along the cue direction
        // Project backwards from ball center along cue direction
        Vector3 contactPoint = ballCenter - cueForward.normalized * ballRadius;
        
        // Adjust contact point Y to match where cue tip actually is
        contactPoint.y = cueTipPos.y;
        
        // 5. Calculate horizontal offset for potential side spin (future feature)
        Vector3 tipToBallHorizontal = new Vector3(
            ballCenter.x - cueTipPos.x,
            0,
            ballCenter.z - cueTipPos.z
        );
        float horizontalOffsetMag = tipToBallHorizontal.magnitude / ballRadius;

        // 6. Get shot power
        float hitSpeed = currentVelocity.magnitude;
        if (hitSpeed < minHitSpeed) hitSpeed = minHitSpeed;

        // 7. Base horizontal direction - use cue's forward direction
        Vector3 horizontalDirection = cueForward;
        horizontalDirection.y = 0;
        horizontalDirection.Normalize();

        // 8. Calculate force direction with optional jump component
        Vector3 forceDirection = horizontalDirection;
        float totalPower = hitSpeed * forceMultiplier;

        // Determine shot type based on vertical offset
        string shotType = "FLAT";
        
        // JUMP SHOT: Only if hit BELOW center AND below threshold
        if (verticalOffset < -jumpThreshold)
        {
            // The lower the hit, the more upward force
            float jumpAmount = Mathf.Abs(verticalOffset + jumpThreshold); // 0 to ~0.7
            float upwardForce = jumpAmount * jumpForceMultiplier;
            
            forceDirection = (horizontalDirection + Vector3.up * upwardForce).normalized;
            shotType = "JUMP";
            
            Debug.Log($"🔺 JUMP SHOT! VOffset: {verticalOffset:F2}, HOffset: {horizontalOffsetMag:F2}, Upward: {upwardForce:F2}");
        }
        else if (verticalOffset < -flatShotThreshold)
        {
            shotType = "BACKSPIN";
        }
        else if (verticalOffset > flatShotThreshold)
        {
            shotType = "TOPSPIN";
        }

        // 10. Apply force at contact point (creates realistic torque)
        ballRb.AddForceAtPosition(
            forceDirection * totalPower,
            contactPoint,
            ForceMode.Impulse
        );

        // 11. Add explicit spin based on vertical hit location
        ApplySpin(ballRb, horizontalDirection, verticalOffset);

        // 12. Debug visualization
        if (showContactPoint)
        {
            Debug.DrawLine(ballCenter, contactPoint, Color.yellow, 2f);
            Debug.DrawRay(contactPoint, forceDirection * 0.5f, Color.red, 2f);
            Debug.DrawRay(ballCenter, cueForward * 0.3f, Color.cyan, 2f);
            
            // Draw the vertical offset
            Debug.DrawLine(cueTipPos, new Vector3(cueTipPos.x, ballCenter.y, cueTipPos.z), Color.magenta, 2f);
        }

        Debug.Log($"⚡ {shotType} | Power: {totalPower:F1} | VOffset: {verticalOffset:F2} | HOffset: {horizontalOffsetMag:F2} | TipY: {cueTipPos.y:F3} | BallY: {ballCenter.y:F3}");

        // 9. Notify CueStickController to hide the stick
        if (cueController != null)
        {
            cueController.NotifyBallHit();
        }
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
        
        // Only apply spin if outside the flat shot zone
        float spinAmount = 0f;
        if (Mathf.Abs(verticalOffset) > flatShotThreshold)
        {
            spinAmount = -verticalOffset * spinStrength; // Negative because backspin opposes motion
        }
        
        ballRb.angularVelocity = spinAxis * spinAmount;

        // Debug spin type
        string spinType = "FLAT";
        if (verticalOffset < -flatShotThreshold) spinType = "BACKSPIN";
        else if (verticalOffset > flatShotThreshold) spinType = "TOPSPIN";
        
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