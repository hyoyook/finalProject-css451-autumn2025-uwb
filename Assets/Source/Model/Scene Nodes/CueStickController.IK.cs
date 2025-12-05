using UnityEngine;

/// <summary>
/// Partial class for CueStickController - Inverse Kinematics for arm joints
/// 
/// Solves the 2-joint IK problem to keep the arm connected when pitching the hand.
/// Assumes a 3-node chain: Shoulder (root) -> Elbow -> Hand (target)
/// </summary>
public partial class CueStickController
{
    [Header("Inverse Kinematics")]
    [Tooltip("Enable IK to keep arm joints connected")]
    public bool EnableIK = true;

    [Tooltip("Upper arm SceneNode (shoulder to elbow)")]
    public SceneNode UpperArmNode;

    [Tooltip("Lower arm SceneNode (elbow to hand)")]
    public SceneNode LowerArmNode;

    [Tooltip("Length of upper arm segment")]
    public float UpperArmLength = 1.0f;

    [Tooltip("Length of lower arm segment")]
    public float LowerArmLength = 1.0f;

    /// <summary>
    /// Solve 2-joint IK to position elbow and shoulder
    /// Called after UpdateCuePosition to adjust arm joints
    /// </summary>
    private void SolveArmIK()
    {
        if (!EnableIK || UpperArmNode == null || LowerArmNode == null || deepestNode == null)
            return;

        Debug.Log("Solving Arm IK");

        // Get positions
        Vector3 shoulderPos = CueHierarchy.transform.position; // Root (shoulder)
        Vector3 handPos = deepestNode.transform.position; // Target (hand)

        // Direction from shoulder to hand
        Vector3 shoulderToHand = handPos - shoulderPos;
        float targetDistance = shoulderToHand.magnitude;

        // Check if target is reachable
        float maxReach = UpperArmLength + LowerArmLength;
        Debug.Log($"IK Target Distance: {targetDistance}, Max Reach: {maxReach}");
        if (targetDistance > maxReach)
        {
            // Target too far - stretch arm straight
            Vector3 direction = shoulderToHand.normalized;
            
            // Position upper arm
            UpperArmNode.transform.localPosition = Vector3.zero;
            UpperArmNode.transform.localRotation = Quaternion.LookRotation(direction);

            // Position lower arm at end of upper arm
            LowerArmNode.transform.localPosition = Vector3.forward * UpperArmLength;
            LowerArmNode.transform.localRotation = Quaternion.identity;

            return;
        }

        // Use law of cosines to find elbow angle
        float upperSq = UpperArmLength * UpperArmLength;
        float lowerSq = LowerArmLength * LowerArmLength;
        float targetSq = targetDistance * targetDistance;

        // Angle at elbow
        float cosElbowAngle = (upperSq + lowerSq - targetSq) / (2 * UpperArmLength * LowerArmLength);
        cosElbowAngle = Mathf.Clamp(cosElbowAngle, -1f, 1f);
        float elbowAngle = Mathf.Acos(cosElbowAngle);

        // Angle at shoulder
        float cosShoulderAngle = (upperSq + targetSq - lowerSq) / (2 * UpperArmLength * targetDistance);
        cosShoulderAngle = Mathf.Clamp(cosShoulderAngle, -1f, 1f);
        float shoulderAngle = Mathf.Acos(cosShoulderAngle);

        // Get the direction to the target
        Vector3 toTarget = shoulderToHand.normalized;

        // Calculate the plane normal (perpendicular to shoulder-hand line)
        // We want the elbow to bend in a specific direction (typically "up" relative to the arm)
        Vector3 bendDirection = Vector3.up; // You can adjust this for different bend planes
        Vector3 elbowPlaneNormal = Vector3.Cross(toTarget, bendDirection).normalized;
        
        // Handle degenerate case where target is directly up/down
        if (elbowPlaneNormal.sqrMagnitude < 0.001f)
        {
            elbowPlaneNormal = Vector3.right;
        }

        // Rotate the target direction by the shoulder angle around the plane normal
        Vector3 upperArmDirection = Quaternion.AngleAxis(-shoulderAngle * Mathf.Rad2Deg, elbowPlaneNormal) * toTarget;

        // Set upper arm rotation
        UpperArmNode.transform.localPosition = Vector3.zero;
        UpperArmNode.transform.localRotation = Quaternion.LookRotation(upperArmDirection);

        // Calculate elbow position
        Vector3 elbowPos = shoulderPos + upperArmDirection * UpperArmLength;

        // Lower arm points from elbow to hand
        Vector3 lowerArmDirection = (handPos - elbowPos).normalized;

        // Set lower arm position and rotation
        LowerArmNode.transform.localPosition = Vector3.forward * UpperArmLength;
        LowerArmNode.transform.localRotation = Quaternion.LookRotation(lowerArmDirection) * Quaternion.Inverse(UpperArmNode.transform.localRotation);
    }

    /// <summary>
    /// Auto-calculate arm segment lengths from initial positions
    /// Call this from Start() after hierarchy is discovered
    /// 
    /// Since SceneNodes all have localPosition = (0,0,0), we calculate lengths
    /// based on the world positions of the nodes (which reflect the primitive positions).
    /// </summary>
    private void CalculateArmLengths()
    {
        if (UpperArmNode == null || LowerArmNode == null || deepestNode == null)
        {
            Debug.LogWarning("Cannot calculate arm lengths - nodes not assigned!");
            return;
        }

        // Calculate based on world positions after the scene is set up
        // This should be called after UpdateCuePosition() has positioned the hierarchy
        
        // Upper arm length: distance from root (shoulder) to UpperArmNode (elbow)
        Vector3 shoulderWorldPos = CueHierarchy.transform.position;
        Vector3 elbowWorldPos = UpperArmNode.transform.position;
        UpperArmLength = Vector3.Distance(shoulderWorldPos, elbowWorldPos);

        // Lower arm length: distance from UpperArmNode (elbow) to deepestNode (hand)
        Vector3 handWorldPos = deepestNode.transform.position;
        LowerArmLength = Vector3.Distance(elbowWorldPos, handWorldPos);

        Debug.Log($"=== Arm Length Calculation ===");
        Debug.Log($"Shoulder world pos: {shoulderWorldPos}");
        Debug.Log($"Elbow world pos: {elbowWorldPos}");
        Debug.Log($"Hand world pos: {handWorldPos}");
        Debug.Log($"Upper Arm Length: {UpperArmLength}");
        Debug.Log($"Lower Arm Length: {LowerArmLength}");
        Debug.Log($"Total Reach: {UpperArmLength + LowerArmLength}");

        // If lengths are still near zero, it means the nodes haven't been positioned yet
        // or the structure needs manual configuration
        if (UpperArmLength < 0.01f || LowerArmLength < 0.01f)
        {
            Debug.LogWarning("Arm lengths calculated as near zero!");
            Debug.LogWarning("This likely means CalculateArmLengths() was called before the hierarchy was positioned.");
            Debug.LogWarning("You should manually set UpperArmLength and LowerArmLength in the Inspector.");
            Debug.LogWarning("Measure the distance between the green spheres in your scene view:");
            Debug.LogWarning("  - UpperArmLength = distance from shoulder (top sphere) to elbow (middle sphere)");
            Debug.LogWarning("  - LowerArmLength = distance from elbow (middle sphere) to hand (bottom sphere)");
            
            // Keep the inspector values (don't override with calculated zero)
            Debug.LogWarning($"Using Inspector values - Upper: {UpperArmLength}, Lower: {LowerArmLength}");
        }
    }
}
