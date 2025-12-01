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

    // Shot state
    private float currentDrawDistance = 0.0f;
    private float chargedPower = 0.0f;
    private bool isCharging = false;
    private bool isStriking = false;
    private bool hasHitBall = false; // Track if we've hit the ball during strike
    private Vector3 originalNodePosition;
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
        // Get the deepest node from hierarchy system
        SceneNode drawNode = GetDeepestNode();

        if (drawNode == null || CueTip == null || CueBallTarget == null)
            return;

        // Store original position on first frame
        if (!hasStoredOriginalPosition)
        {
            originalNodePosition = drawNode.transform.localPosition;
            hasStoredOriginalPosition = true;

            if (ShowHierarchyDebug)
            {
                Debug.Log($"Shot system using node: {drawNode.name}");
            }
        }

        if (Keyboard.current == null)
            return;

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

        // Move node backward along the draw axis
        Vector3 drawOffset = DrawAxis.normalized * currentDrawDistance;
        drawNode.transform.localPosition = originalNodePosition + drawOffset;

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

            // Update node position (works for both positive and negative draw distances)
            Vector3 drawOffset = DrawAxis.normalized * currentDrawDistance;
            drawNode.transform.localPosition = originalNodePosition + drawOffset;

            // Check for collision with cue ball
            if (CheckCueBallHit())
            {
                OnCueBallHit(drawNode);
                return;
            }

            // Safety: limit how far past original we can go (to avoid going through the ball)
            float maxForwardDistance = -MaxDrawDistance; // Can go as far forward as we could go back
            if (currentDrawDistance < maxForwardDistance)
            {
                if (ShowHierarchyDebug)
                {
                    Debug.Log("Strike missed - reached max forward distance without hitting ball");
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

        // Update position
        Vector3 drawOffset = DrawAxis.normalized * currentDrawDistance;
        drawNode.transform.localPosition = originalNodePosition + drawOffset;

        // Check if we're back at original position
        if (Mathf.Approximately(currentDrawDistance, 0))
        {
            drawNode.transform.localPosition = originalNodePosition;
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
    /// Called when cue tip hits the cue ball
    /// </summary>
    private void OnCueBallHit(SceneNode drawNode)
    {
        Debug.Log($"*** CUE BALL HIT! Power: {chargedPower:F1} ***");

        // Calculate strike direction (from cue tip toward ball center)
        Vector3 strikeDirection = (CueBallTarget.position - CueTip.position).normalized;

        // Apply force to cue ball if it has a Rigidbody
        Rigidbody ballRb = CueBallTarget.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.AddForce(strikeDirection * chargedPower, ForceMode.Impulse);
            Debug.Log($"Applied force: {strikeDirection * chargedPower}");
        }

        // Mark that we've hit the ball - this triggers the return phase
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
            Debug.Log("Strike complete");
        }
    }

    /// <summary>
    /// Reset shot state (useful for repositioning)
    /// </summary>
    public void ResetShot()
    {
        SceneNode drawNode = GetDeepestNode();
        if (drawNode != null)
        {
            EndStrike(drawNode);
            originalNodePosition = drawNode.transform.localPosition;
        }
    }

    // Public accessors
    public float CurrentDrawDistance => currentDrawDistance;
    public float ChargedPower => chargedPower;
    public bool IsCharging => isCharging;
    public bool IsStriking => isStriking;
    public float DrawPercentage => MaxDrawDistance > 0 ? currentDrawDistance / MaxDrawDistance : 0;
}
