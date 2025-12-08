using UnityEngine;
using UnityEngine.InputSystem;

/// 
/// Controls:
/// - Hold SPACE: Draw cue back (charge shot) - moves deepest node
/// - Release SPACE: Strike forward until cue tip hits ball
/// </summary>
public partial class CueStickController
{
    [Header("Shot Settings")]
    [Tooltip("Maximum draw distance")]
    public float MaxDrawDistance = 2.0f;

    [Tooltip("Speed of drawing back the cue")]
    public float DrawSpeed = 3.0f;

    [Tooltip("Speed of striking forward")]
    public float StrikeSpeed = 15.0f;

    [Tooltip("Power multiplier based on draw distance")]
    public float PowerMultiplier = 10.0f;

    [Tooltip("Which axis to draw along (local space of the node)")]
    public Vector3 DrawAxis = new Vector3(1, 0, 0); // Default: +X

    [Header("Jump Shot Settings")]
    [Tooltip("Current elevation angle of cue (0 = horizontal, + = angled down for jump)")]
    [Range(-30f, 30f)]
    public float CueElevation = 0f;

    [Tooltip("Speed of elevation adjustment (Q/E keys)")]
    public float ElevationSpeed = 30f;

    [Header("Spin Settings")]
    [Tooltip("Backspin/Topspin strength (0 = none, higher = more spin)")]
    [Range(0f, 50f)]
    public float SpinStrength = 20f;

    [Tooltip("Current spin setting (-1 = backspin, 0 = none, +1 = topspin)")]
    [Range(-1f, 1f)]
    public float SpinAmount = 0f;

    [Tooltip("Speed of spin adjustment (W/S keys)")]
    public float SpinAdjustSpeed = 1f;

    // Shot state
    private float currentDrawDistance = 0.0f;
    private float chargedPower = 0.0f;
    private bool isCharging = false;
    private bool isStriking = false;
    private bool hasHitBall = false; // Track if we've hit the ball during strike
    private Vector3 originalNodePosition;
    private Quaternion originalNodeRotation; // Store original rotation too
    private bool hasStoredOriginalPosition = false;

    /// <summary>
    /// Initialize shot system - call from Start()
    /// </summary>
    private void ShotStart()
    {
        // Position will be stored on first use after hierarchy is built
        hasStoredOriginalPosition = false;
    }

    /// <summary>
    /// Update shot system - call from Update()
    /// </summary>
    private void UpdateShot()
    {
        // Get the deepest node (Cue stick) from hierarchy system
        SceneNode drawNode = GetDeepestNode();

        if (drawNode == null || CueTip == null || CueBallTarget == null)
            return;

        // Store original position and rotation when not in use
        if (!hasStoredOriginalPosition && !isCharging && !isStriking)
        {
            originalNodePosition = drawNode.transform.localPosition;
            originalNodeRotation = drawNode.transform.localRotation;
            hasStoredOriginalPosition = true;

            if (ShowHierarchyDebug)
            {
                Debug.Log($"Shot system using node: {drawNode.name}");
            }
        }
        
        // Update stored rotation whenever we're not charging/striking (allows aiming adjustments)
        if (!isCharging && !isStriking)
        {
            originalNodeRotation = drawNode.transform.localRotation;
        }

        if (Keyboard.current == null)
            return;

        // Handle cue elevation adjustment (Q/E keys) - only when not charging/striking
        if (!isCharging && !isStriking)
        {
            if (Keyboard.current.qKey.isPressed)
            {
                CueElevation -= ElevationSpeed * Time.deltaTime;
            }
            if (Keyboard.current.eKey.isPressed)
            {
                CueElevation += ElevationSpeed * Time.deltaTime;
            }
            CueElevation = Mathf.Clamp(CueElevation, -30f, 30f);
        }

        // Handle spin adjustment (W/S keys) - only when not charging/striking
        if (!isCharging && !isStriking)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                SpinAmount += SpinAdjustSpeed * Time.deltaTime;
            }
            if (Keyboard.current.sKey.isPressed)
            {
                SpinAmount -= SpinAdjustSpeed * Time.deltaTime;
            }
            SpinAmount = Mathf.Clamp(SpinAmount, -1f, 1f);
        }

        // Handle charging (holding space)
        if (Keyboard.current.spaceKey.isPressed && !isStriking)
        {
            isCharging = true;
            ChargeShot(drawNode);
        }
        // Handle release (start strike)
        else if (Keyboard.current.spaceKey.wasReleasedThisFrame && isCharging)
        {
            isCharging = false;
            StartStrike();
        }

        // Handle strike animation
        if (isStriking)
        {
            UpdateStrike(drawNode);
        }
    }

    /// <summary>
    /// Charge the shot by drawing the node back
    /// </summary>
    private void ChargeShot(SceneNode drawNode)
    {
        // Increase draw distance
        currentDrawDistance += DrawSpeed * Time.deltaTime;
        currentDrawDistance = Mathf.Min(currentDrawDistance, MaxDrawDistance);

        // Calculate power based on draw distance
        chargedPower = (currentDrawDistance / MaxDrawDistance) * PowerMultiplier;

        // Calculate draw direction in local space, accounting for the node's rotation
        // The draw axis is rotated by the stored rotation (which includes yaw/pitch adjustments)
        Vector3 rotatedDrawAxis = originalNodeRotation * DrawAxis.normalized;
        Vector3 drawOffset = rotatedDrawAxis * currentDrawDistance;
        
        // Apply position offset and maintain rotation
        drawNode.transform.localPosition = originalNodePosition + drawOffset;
        drawNode.transform.localRotation = originalNodeRotation;

        // Debug
        if (ShowHierarchyDebug)
        {
            Debug.Log($"Charging: {currentDrawDistance:F2} / {MaxDrawDistance:F2} | Power: {chargedPower:F1}");
        }
    }

    /// <summary>
    /// Start the strike after releasing space
    /// </summary>
    private void StartStrike()
    {
        isStriking = true;
        if (ShowHierarchyDebug)
        {
            Debug.Log($"Strike started with power: {chargedPower:F1}");
        }
    }

    /// <summary>
    /// Update the strike animation - move forward until hitting the ball, then return
    /// </summary>
    private void UpdateStrike(SceneNode drawNode)
    {
        if (!hasHitBall)
        {
            // Phase 1: Move forward until we hit the ball
            // Continue moving past original position (into negative draw distance) if needed
            currentDrawDistance -= StrikeSpeed * Time.deltaTime;

            // Calculate draw direction in local space, accounting for the node's rotation
            Vector3 rotatedDrawAxis = originalNodeRotation * DrawAxis.normalized;
            Vector3 drawOffset = rotatedDrawAxis * currentDrawDistance;
            
            // Update node position and maintain rotation
            drawNode.transform.localPosition = originalNodePosition + drawOffset;
            drawNode.transform.localRotation = originalNodeRotation;

            // Check for collision with cue ball
            // NOTE: Hit detection is now handled by CueTipHit.cs (trigger-based)
            // This manual check is disabled to avoid double-hits
            // if (CheckCueBallHit())
            // {
            //     OnCueBallHit(drawNode);
            //     return;
            // }

            // Safety: limit how far past original we can go (to avoid going through the ball)
            float maxForwardDistance = -MaxDrawDistance; // Can go as far forward as we could go back
            if (currentDrawDistance < maxForwardDistance)
            {
                if (ShowHierarchyDebug)
                {
                    // Debug.Log("Strike missed - reached max forward distance without hitting ball");
                }
                // Start returning to original position
                hasHitBall = true; // Use this flag to trigger return phase
            }
        }
        else
        {
            // Phase 2: Return to original position after hitting ball (or missing)
            ReturnToOriginalPosition(drawNode);
        }
    }

    /// <summary>
    /// Smoothly return the node to its original position after a strike
    /// </summary>
    private void ReturnToOriginalPosition(SceneNode drawNode)
    {
        // Move toward zero draw distance
        if (currentDrawDistance < 0)
        {
            currentDrawDistance += StrikeSpeed * Time.deltaTime;
            currentDrawDistance = Mathf.Min(currentDrawDistance, 0); // Don't overshoot
        }
        else if (currentDrawDistance > 0)
        {
            currentDrawDistance -= StrikeSpeed * Time.deltaTime;
            currentDrawDistance = Mathf.Max(currentDrawDistance, 0); // Don't overshoot
        }

        // Calculate draw direction in local space, accounting for the node's rotation
        Vector3 rotatedDrawAxis = originalNodeRotation * DrawAxis.normalized;
        Vector3 drawOffset = rotatedDrawAxis * currentDrawDistance;
        
        // Update position and maintain rotation
        drawNode.transform.localPosition = originalNodePosition + drawOffset;
        drawNode.transform.localRotation = originalNodeRotation;

        // Check if we're back at original position
        if (Mathf.Approximately(currentDrawDistance, 0))
        {
            drawNode.transform.localPosition = originalNodePosition;
            drawNode.transform.localRotation = originalNodeRotation;
            EndStrike(drawNode);
        }
    }

    /// <summary>
    /// Check if the cue tip has hit the cue ball
    /// </summary>
    private bool CheckCueBallHit()
    {
        if (CueTip == null || CueBallTarget == null)
            return false;

        Collider ballCollider = CueBallTarget.GetComponent<Collider>();
        if (ballCollider == null)
        {
            // Fallback: distance check
            float distance = Vector3.Distance(CueTip.position, CueBallTarget.position);
            float ballRadius = CueBallTarget.localScale.x * 0.5f;
            return distance <= ballRadius;
        }

        // Check if cue tip is inside or touching the ball collider
        Vector3 closestPoint = ballCollider.ClosestPoint(CueTip.position);
        float distToClosest = Vector3.Distance(CueTip.position, closestPoint);

        return distToClosest < 0.05f; // Small threshold
    }

    /// <summary>
    /// Called when cue tip hits the cue ball - REALISTIC CONTACT POINT PHYSICS
    /// Jump shots happen when hitting BELOW center, spin from hit height
    /// </summary>
    private void OnCueBallHit(SceneNode drawNode)
    {
        Rigidbody ballRb = CueBallTarget.GetComponent<Rigidbody>();
        if (ballRb == null) return;

        ballRb.isKinematic = false;

        // === 1. CALCULATE CONTACT POINT ===
        Vector3 ballCenter = CueBallTarget.position;
        Vector3 cueTipPos = CueTip.position;
        float ballRadius = CueBallTarget.localScale.x * 0.5f;

        // Find where cue tip touches ball surface
        Vector3 toBall = ballCenter - cueTipPos;
        Vector3 contactPoint = cueTipPos + toBall.normalized * toBall.magnitude;

        // === 2. VERTICAL OFFSET (-1 = bottom, 0 = center, +1 = top) ===
        float verticalOffset = (contactPoint.y - ballCenter.y) / ballRadius;
        verticalOffset = Mathf.Clamp(verticalOffset, -1f, 1f);

        // === 3. HORIZONTAL DIRECTION (flattened) ===
        Vector3 horizontalDir = toBall;
        horizontalDir.y = 0;
        horizontalDir.Normalize();

        // === 4. CALCULATE FORCE DIRECTION ===
        Vector3 forceDirection = horizontalDir;
 
        // JUMP SHOT: Only if hit BELOW center (negative offset) and below threshold
        float jumpThreshold = 0.25f; // Must hit lower than this to jump
        if (verticalOffset < -jumpThreshold)
        {
            // The lower the hit, the more upward force
            float jumpAmount = Mathf.Abs(verticalOffset + jumpThreshold); // 0 to ~0.75
            float upwardComponent = jumpAmount * 2.5f; // Jump force multiplier
            
            forceDirection = (horizontalDir + Vector3.up * upwardComponent).normalized;
            
            Debug.Log($"🏀 JUMP SHOT! Vertical Offset: {verticalOffset:F2}, Upward: {upwardComponent:F2}");
        }

        // === 5. APPLY FORCE AT CONTACT POINT ===
        // AddForceAtPosition automatically creates realistic torque/spin
        Vector3 forceVector = forceDirection * chargedPower;
        ballRb.AddForceAtPosition(forceVector, contactPoint, ForceMode.Impulse);

        // === 6. EXPLICIT SPIN (enhanced realism) ===
        // Spin based on vertical hit location:
        // - Below center = BACKSPIN (negative)
        // - Above center = TOPSPIN (positive)
        // - At center = NO SPIN
        Vector3 spinAxis = Vector3.Cross(horizontalDir, Vector3.up).normalized;
        float spinMagnitude = -verticalOffset * SpinStrength; // Negative = backspin opposes motion
        ballRb.angularVelocity = spinAxis * spinMagnitude;

        // === 7. DEBUG OUTPUT ===
        string spinType = "FLAT";
        if (verticalOffset < -0.1f) spinType = "BACKSPIN";
        else if (verticalOffset > 0.1f) spinType = "TOPSPIN";
        
        Debug.Log($"⚡ HIT! Power: {chargedPower:F1} | Offset: {verticalOffset:F2} | {spinType} | Spin: {spinMagnitude:F1} rad/s");
        
        // Visual debug lines
        if (ShowHierarchyDebug)
        {
            Debug.DrawLine(ballCenter, contactPoint, Color.yellow, 2f);
            Debug.DrawRay(contactPoint, forceDirection * 0.5f, Color.red, 2f);
            Debug.DrawRay(ballCenter, spinAxis * 0.3f, Color.cyan, 2f);
        }

        // === 8. HIDE STICK ===
        if (CueHierarchy != null)
        {
            CueHierarchy.gameObject.SetActive(false);
        }

        hasHitBall = true;
    }

    /// <summary>
    /// End the strike and reset state
    /// </summary>
    private void EndStrike(SceneNode drawNode)
    {
        isStriking = false;
        isCharging = false;
        hasHitBall = false;
        currentDrawDistance = 0;
        chargedPower = 0;

        // Ensure node is at original position
        if (drawNode != null)
        {
            drawNode.transform.localPosition = originalNodePosition;
        }

        if (ShowHierarchyDebug)
        {
            // Debug.Log("Strike complete");
        }
    }

    /// <summary>
    /// Reset shot state (return node to original position)
    /// </summary>
    public void ResetShot()
    {
        SceneNode drawNode = GetDeepestNode();
        if (drawNode != null)
        {
            EndStrike(drawNode);
            originalNodePosition = drawNode.transform.localPosition;
            originalNodeRotation = drawNode.transform.localRotation;
        }
    }

    /// <summary>
    /// Called by CueTipHit when a trigger-based collision occurs
    /// This hides the cue stick and marks that the ball was hit
    /// </summary>
    public void NotifyBallHit()
    {
        // Hide the cue stick immediately
        if (CueHierarchy != null)
        {
            CueHierarchy.gameObject.SetActive(false);
        }

        // Mark that we've hit the ball - this triggers the return phase
        hasHitBall = true;

        if (ShowHierarchyDebug)
        {
            Debug.Log("Ball hit notification received from CueTipHit");
        }
    }

    // Public getters
    public float CurrentDrawDistance => currentDrawDistance;
    public float ChargedPower => chargedPower;
    public bool IsCharging => isCharging;
    public bool IsStriking => isStriking;
    public float DrawPercentage => MaxDrawDistance > 0 ? currentDrawDistance / MaxDrawDistance : 0;
    public float CurrentElevation => CueElevation;
    public float CurrentSpin => SpinAmount;
    public string SpinType => SpinAmount < -0.01f ? "Backspin" : SpinAmount > 0.01f ? "Topspin" : "No Spin";
}
