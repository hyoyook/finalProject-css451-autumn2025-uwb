using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the cue stick orbiting around a target (cue ball).
/// 
/// Controls:
/// - WASD: Yaw and Pitch orbiting around the cue ball
/// - CTRL + WASD: Pitch the cue stick in place (adjust angle of attack)
/// </summary>
public partial class CueStickController : MonoBehaviour
{
    [Header("Scene Node References")]
    [Tooltip("The root SceneNode that will orbit around the target")]
    public SceneNode CueHierarchy;

    [Tooltip("The target transform to orbit around (cue ball position)")]
    public Transform CueBallTarget;

    [Header("Orbit Settings")]
    [Tooltip("Distance from the cue ball to orbit at")]
    public float OrbitDistance = 5.0f;

    [Tooltip("Speed of yaw rotation (left/right orbiting)")]
    public float YawSpeed = 60.0f;

    [Tooltip("Speed of pitch rotation (up/down orbiting)")]
    public float PitchSpeed = 40.0f;

    [Tooltip("Minimum pitch angle (looking down at the ball)")]
    public float MinPitchAngle = 5.0f;

    [Tooltip("Maximum pitch angle (looking more level)")]
    public float MaxPitchAngle = 60.0f;

    [Header("Cue Pitch Settings (CTRL + WASD)")]
    [Tooltip("Speed of cue stick pitch adjustment")]
    public float CuePitchSpeed = 30.0f;

    [Tooltip("Minimum cue pitch angle")]
    public float MinCuePitch = -15.0f;

    [Tooltip("Maximum cue pitch angle")]
    public float MaxCuePitch = 45.0f;

    // Current orbit angles
    private float currentYaw = 0.0f;
    private float currentPitch = 20.0f;

    // Current cue stick pitch (in-place rotation)
    private float currentCuePitch = 0.0f;

    private void Start()
    {
        // Initialize position based on current settings
        if (CueBallTarget != null && CueHierarchy != null)
        {
            UpdateCuePosition();
        }

        // Initialize hierarchy discovery (from partial class)
        HierarchyStart();

        // Initialize laser pointer (from partial class)
        LaserPointerStart();
        
        // Initialize shot system (from partial class)
        ShotStart();
    }

    private void Update()
    {
        if (CueBallTarget == null || CueHierarchy == null)
            return;

        bool ctrlHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
        // Debug.Log("CTRL Held: " + ctrlHeld);
        if (ctrlHeld)
        {
            // CTRL + WASD: Pitch the cue in place
            HandleCuePitchInput();
        }
        else
        {
            // WASD: Orbit around the cue ball
            HandleOrbitInput();
        }

        // Update the cue stick position and rotation
        UpdateCuePosition();

        // Update the scene node hierarchy
        UpdateHierarchy();

        // Update node selection (from partial class)
        UpdateNodeSelection();

        // Update laser pointer position (from partial class)
        UpdateLaserPointer();
        
        // Update shot system (from partial class)
        UpdateShot();
    }

    /// <summary>
    /// Handles WASD input for orbiting around the cue ball
    /// </summary>
    private void HandleOrbitInput()
    {

        if (Keyboard.current == null)
            return;

        // Debug.Log("Handling Orbit Input");
        float yawInput = 0.0f;
        float pitchInput = 0.0f;

        // A/D for yaw (left/right orbit)
        if (Keyboard.current.aKey.isPressed)
        {
            // Debug.Log("A Key Pressed for Yaw");
            yawInput = -1.0f;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            // Debug.Log("D Key Pressed for Yaw");
            yawInput = 1.0f;
        }
        // W/S for pitch (up/down orbit)
        if (Keyboard.current.wKey.isPressed)
        {
            // Debug.Log("W Key Pressed for Pitch");
            pitchInput = 1.0f;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            // Debug.Log("S Key Pressed for Pitch");
            pitchInput = -1.0f;

        }

        // Apply yaw rotation
        currentYaw += yawInput * YawSpeed * Time.deltaTime;
        currentYaw = NormalizeAngle(currentYaw);

        // Apply pitch rotation with clamping
        currentPitch += pitchInput * PitchSpeed * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, MinPitchAngle, MaxPitchAngle);




    }

    /// <summary>
    /// Handles CTRL + WASD input for pitching the cue stick in place
    /// </summary>
    private void HandleCuePitchInput()
    {
        // Debug.Log("Handling Cue Pitch Input");
        if (Keyboard.current == null)
            return;

        float pitchInput = 0.0f;

        // W/S for cue pitch adjustment
        if (Keyboard.current.wKey.isPressed)
        {
            // Debug.Log("W Key Pressed for Cue Pitch");
            pitchInput = 1.0f;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            // Debug.Log("S Key Pressed for Cue Pitch");
            pitchInput = -1.0f;
        }

        // Apply cue pitch with clamping
        currentCuePitch += pitchInput * CuePitchSpeed * Time.deltaTime;
        currentCuePitch = Mathf.Clamp(currentCuePitch, MinCuePitch, MaxCuePitch);
    }

    /// <summary>
    /// Updates the cue stick position based on current orbit angles
    /// </summary>
    private void UpdateCuePosition()
    {
        // Calculate the position on the orbit sphere
        float yawRad = currentYaw * Mathf.Deg2Rad;
        float pitchRad = currentPitch * Mathf.Deg2Rad;

        // Calculate offset from target (spherical coordinates)
        float x = OrbitDistance * Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
        float y = OrbitDistance * Mathf.Sin(pitchRad);
        float z = OrbitDistance * Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);

        Vector3 offset = new Vector3(x, y, z);
        Vector3 targetPos = CueBallTarget.position;

        // Set the cue stick root position
        CueHierarchy.transform.position = targetPos + offset;

        // Calculate the direction to the cue ball
        Vector3 directionToTarget = (targetPos - CueHierarchy.transform.position).normalized;

        // We want -right (negative X) to point at the ball
        // So we need forward to be perpendicular to that direction
        // Right = -directionToTarget, so Forward = cross(Up, Right) = cross(Up, -directionToTarget)
        Vector3 desiredRight = directionToTarget;
        Vector3 desiredUp = Vector3.up;
        Vector3 desiredForward = Vector3.Cross(desiredUp, desiredRight).normalized;

        // Handle case where direction is straight up/down
        if (desiredForward.sqrMagnitude < 0.001f)
        {
            desiredForward = Vector3.forward;
        }

        // Recalculate up to ensure orthogonality
        desiredUp = Vector3.Cross(desiredRight, desiredForward).normalized;

        // Create rotation from the three axes
        Quaternion baseRotation = Quaternion.LookRotation(desiredForward, desiredUp);

        // Apply the in-place cue pitch adjustment (rotate around local forward axis)
        Quaternion cuePitchRotation = Quaternion.AngleAxis(currentCuePitch, Vector3.forward);

        CueHierarchy.transform.rotation = baseRotation * cuePitchRotation;
    }

    /// <summary>
    /// Updates the SceneNode hierarchy
    /// </summary>
    private void UpdateHierarchy()
    {
        Matrix4x4 identity = Matrix4x4.identity;
        CueHierarchy.CompositeXform(ref identity);

        // Debug: Verify hierarchy is being updated
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            // Debug.Log("=== SceneNode Hierarchy Debug ===");
            // Debug.Log($"Root Position: {CueHierarchy.transform.position}");
            // Debug.Log($"Root Rotation: {CueHierarchy.transform.rotation.eulerAngles}");
            DebugPrintHierarchy(CueHierarchy, 0);
        }
    }

    /// <summary>
    /// Recursively prints the SceneNode hierarchy for debugging
    /// </summary>
    private void DebugPrintHierarchy(SceneNode node, int depth)
    {
        string indent = new string('-', depth * 2);
        // Debug.Log($"{indent} SceneNode: {node.name}");
        // Debug.Log($"{indent}   NodeOrigin: {node.NodeOrigin}");
        // Debug.Log($"{indent}   LocalPos: {node.transform.localPosition}");
        // Debug.Log($"{indent}   LocalRot: {node.transform.localRotation.eulerAngles}");

        // Print primitives
        if (node.PrimitiveList != null)
        {
            Debug.Log($"{indent}   Primitives: {node.PrimitiveList.Count}");
            foreach (var prim in node.PrimitiveList)
            {
                if (prim != null)
                    Debug.Log($"{indent}     - {prim.name}");
            }
        }

        // Recurse to children
        if (node.ChildrenList != null)
        {
            foreach (var child in node.ChildrenList)
            {
                if (child != null)
                    DebugPrintHierarchy(child, depth + 1);
            }
        }
    }

    /// <summary>
    /// Normalizes an angle to be within -180 to 180 degrees
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        while (angle > 180.0f) angle -= 360.0f;
        while (angle < -180.0f) angle += 360.0f;
        return angle;
    }

    /// <summary>
    /// Sets the orbit angles directly (useful for resetting or scripted movements)
    /// </summary>
    public void SetOrbitAngles(float yaw, float pitch)
    {
        currentYaw = NormalizeAngle(yaw);
        currentPitch = Mathf.Clamp(pitch, MinPitchAngle, MaxPitchAngle);
    }

    /// <summary>
    /// Sets the cue pitch angle directly
    /// </summary>
    public void SetCuePitch(float pitch)
    {
        currentCuePitch = Mathf.Clamp(pitch, MinCuePitch, MaxCuePitch);
    }

    /// <summary>
    /// Resets all angles to default values
    /// </summary>
    public void ResetAngles()
    {
        currentYaw = 0.0f;
        currentPitch = 20.0f;
        currentCuePitch = 0.0f;
    }

    /// <summary>
    /// Gets the current direction the cue is pointing (towards the ball)
    /// </summary>
    public Vector3 GetCueDirection()
    {
        if (CueBallTarget != null && CueHierarchy != null)
        {
            return (CueBallTarget.position - CueHierarchy.transform.position).normalized;
        }
        return Vector3.forward;
    }

    /// <summary>
    /// Gets the current yaw angle
    /// </summary>
    public float GetCurrentYaw() => currentYaw;

    /// <summary>
    /// Gets the current pitch angle
    /// </summary>
    public float GetCurrentPitch() => currentPitch;

    /// <summary>
    /// Gets the current cue pitch angle
    /// </summary>
    public float GetCurrentCuePitch() => currentCuePitch;
}