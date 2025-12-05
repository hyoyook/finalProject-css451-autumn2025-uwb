using UnityEngine;

[ExecuteInEditMode]
public class SimpleIK : MonoBehaviour
{
    [Header("Scene Nodes")]
    public SceneNode ShoulderNode;  // Parent joint
    public SceneNode ElbowNode;     // Middle joint
    
    [Header("IK Target")]
    public Transform Target;        // World-space target
    
    [Header("Arm Lengths")]
    public float UpperArmLength = 2f;
    public float ForearmLength = 2f;
    
    [Header("Options")]
    public bool ElbowBendPositive = true;
    
    [Header("Axis Configuration")]
    public Axis ForwardAxis = Axis.X;
    public Axis UpAxis = Axis.Y;
    
    public enum Axis { X, Y, Z, NegX, NegY, NegZ }

    void Update()
    {
        if (ShoulderNode == null || ElbowNode == null || Target == null) return;
        
        SolveIK();
    }
    
    float GetAxisValue(Vector3 v, Axis axis)
    {
        switch (axis)
        {
            case Axis.X: return v.x;
            case Axis.Y: return v.y;
            case Axis.Z: return v.z;
            case Axis.NegX: return -v.x;
            case Axis.NegY: return -v.y;
            case Axis.NegZ: return -v.z;
            default: return v.z;
        }
    }
    
    // Returns which Unity axis to rotate around based on forward/up selection
    Vector3 GetRotationAxis()
    {
        // If forward is X and up is Y, we rotate around Z
        // If forward is Z and up is Y, we rotate around X
        // If forward is X and up is Z, we rotate around Y
        
        bool forwardIsX = (ForwardAxis == Axis.X || ForwardAxis == Axis.NegX);
        bool forwardIsY = (ForwardAxis == Axis.Y || ForwardAxis == Axis.NegY);
        bool forwardIsZ = (ForwardAxis == Axis.Z || ForwardAxis == Axis.NegZ);
        
        bool upIsX = (UpAxis == Axis.X || UpAxis == Axis.NegX);
        bool upIsY = (UpAxis == Axis.Y || UpAxis == Axis.NegY);
        bool upIsZ = (UpAxis == Axis.Z || UpAxis == Axis.NegZ);
        
        // Rotation axis is the one that's neither forward nor up
        if (!forwardIsX && !upIsX) return Vector3.right;   // Rotate around X
        if (!forwardIsY && !upIsY) return Vector3.up;      // Rotate around Y
        if (!forwardIsZ && !upIsZ) return Vector3.forward; // Rotate around Z
        
        return Vector3.forward; // Default
    }
    
    void SolveIK()
    {
        // Get shoulder's world position and orientation
        Transform shoulderTransform = ShoulderNode.transform;
        
        // Convert target position to shoulder's LOCAL space
        Vector3 targetLocal = shoulderTransform.InverseTransformPoint(Target.position);
        
        // Get the forward and up components based on configuration
        float targetForward = GetAxisValue(targetLocal, ForwardAxis);
        float targetUp = GetAxisValue(targetLocal, UpAxis);
        
        // Distance in the plane defined by forward and up axes
        float distance = Mathf.Sqrt(targetForward * targetForward + targetUp * targetUp);
        
        // Clamp to reachable range
        float minReach = Mathf.Abs(UpperArmLength - ForearmLength) + 0.001f;
        float maxReach = UpperArmLength + ForearmLength - 0.001f;
        distance = Mathf.Clamp(distance, minReach, maxReach);
        
        // Law of cosines: find elbow angle
        float cosElbow = (UpperArmLength * UpperArmLength + 
                          ForearmLength * ForearmLength - 
                          distance * distance) / 
                         (2f * UpperArmLength * ForearmLength);
        cosElbow = Mathf.Clamp(cosElbow, -1f, 1f);
        float elbowAngle = Mathf.Acos(cosElbow) * Mathf.Rad2Deg;
        
        // Law of cosines: find shoulder offset angle
        float cosShoulder = (distance * distance + 
                             UpperArmLength * UpperArmLength - 
                             ForearmLength * ForearmLength) / 
                            (2f * distance * UpperArmLength);
        cosShoulder = Mathf.Clamp(cosShoulder, -1f, 1f);
        float shoulderOffset = Mathf.Acos(cosShoulder) * Mathf.Rad2Deg;
        
        // Angle to target in the forward-up plane
        float baseAngle = Mathf.Atan2(targetUp, targetForward) * Mathf.Rad2Deg;
        
        // Apply elbow bend direction
        float bendSign = ElbowBendPositive ? 1f : -1f;
        
        // Calculate final angles
        float shoulderRot = baseAngle + (shoulderOffset * bendSign);
        float elbowRot = -bendSign * (180f - elbowAngle);
        
        // Get the rotation axis (perpendicular to the forward-up plane)
        Vector3 rotAxis = GetRotationAxis();
        
        // Apply rotations around the correct axis
        ShoulderNode.transform.localRotation = Quaternion.AngleAxis(shoulderRot, rotAxis);
        ElbowNode.transform.localRotation = Quaternion.AngleAxis(elbowRot, rotAxis);
    }
}