using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class CueStickController : MonoBehaviour
{
    [Header("Scene Node References")]
    public SceneNode CueHierarchy;
    public Transform CueBallTarget;

    [Header("Orbit Settings")]
    public float OrbitDistance = 5.0f;
    public float YawSpeed = 60.0f;
    public float PitchSpeed = 40.0f;
    public float MinPitchAngle = 5.0f;
    public float MaxPitchAngle = 60.0f;

    [Header("Cue Pitch Settings (CTRL + WASD)")]
    public float CuePitchSpeed = 30.0f;
    public float MinCuePitch = -20.0f;
    public float MaxCuePitch = 45.0f;

    private float currentYaw = 0.0f;
    private float currentPitch = 20.0f;
    private float currentCuePitch = 0.0f;

    // References to disable physics
    private Collider[] stickColliders;
    private Rigidbody stickRb;

    private void Start()
    {
        // SAFETY 1: Get all colliders to disable them during aiming
        if (CueHierarchy != null)
        {
            stickColliders = CueHierarchy.GetComponentsInChildren<Collider>();
            stickRb = CueHierarchy.GetComponent<Rigidbody>();
            
            // SAFETY 2: Make stick kinematic so gravity doesn't affect it
            if (stickRb != null) stickRb.isKinematic = true; 
        }

        if (CueBallTarget != null && CueHierarchy != null)
        {
            UpdateCuePosition();
        }

        HierarchyStart();
        LaserPointerStart();
        ShotStart();
    }

    private void Update()
    {
        if (CueBallTarget == null || CueHierarchy == null)
            return;
        
        // SAFETY 3: Force colliders OFF every frame while aiming
        // This prevents the "Invisible Push" where the stick moves the ball
        SetCollidersEnabled(false);

        bool ctrlHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
        // Debug.Log("CTRL Held: " + ctrlHeld);

        if (ctrlHeld)
        {
            HandleCuePitchInput();
        }
        else
        {
            HandleOrbitInput();
        }

        UpdateCuePosition();
        UpdateHierarchy();
        UpdateNodeSelection();
        UpdateLaserPointer();
        UpdateShot();

        // Debug.Log("Current Pitch is: " + currentPitch);
    }

    private void HandleOrbitInput()
    {
        if (Keyboard.current == null) return;

        // Debug.Log("Handling Orbit Input");
        float yawInput = 0.0f;
        float pitchInput = 0.0f;

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

        currentYaw += yawInput * YawSpeed * Time.deltaTime;
        currentYaw = NormalizeAngle(currentYaw);

        currentPitch += pitchInput * PitchSpeed * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, MinPitchAngle, MaxPitchAngle);
    }

    private void HandleCuePitchInput()
    {
        // Debug.Log("Handling Cue Pitch Input");
        if (Keyboard.current == null) return;

        float pitchInput = 0.0f;

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

        currentCuePitch += pitchInput * CuePitchSpeed * Time.deltaTime;
        currentCuePitch = Mathf.Clamp(currentCuePitch, MinCuePitch, MaxCuePitch);
    }

    private void UpdateCuePosition()
    {
        if (CueBallTarget == null || CueHierarchy == null) return;

        Rigidbody ballRb = CueBallTarget.GetComponent<Rigidbody>();
        
        // Check if the ball is moving (or about to move because we just unclamped it)
        bool isBallMoving = ballRb != null && (!ballRb.isKinematic && ballRb.linearVelocity.magnitude > 0.01f);

        if (isBallMoving)
        {
            // 1. DISAPPEAR: If the ball is rolling, hide the entire cue stick hierarchy
            if (CueHierarchy.gameObject.activeSelf)
            {
                CueHierarchy.gameObject.SetActive(false);
            }
            // 2. STOP TRACKING: Don't update position, let it stay "gone"
            return; 
        }
        else
        {
            // 3. REAPPEAR: Ball has stopped, show the stick again for the next shot
            if (!CueHierarchy.gameObject.activeSelf)
            {
                CueHierarchy.gameObject.SetActive(true);
                
                // Optional: Reset orbit angles to behind the ball?
                // currentYaw = 0; 
            }
        }

        // --- Standard Orbit Code Below (Only runs when ball is stopped) ---
        float yawRad = currentYaw * Mathf.Deg2Rad;
        float pitchRad = currentPitch * Mathf.Deg2Rad;

        // (Your existing math...)
        float x = OrbitDistance * Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
        float y = OrbitDistance * Mathf.Sin(pitchRad);
        float z = OrbitDistance * Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);

        Vector3 offset = new Vector3(x, y, z);
        Vector3 targetPos = CueBallTarget.position;

        CueHierarchy.transform.position = targetPos + offset;

        // (Your existing rotation code...)
        Vector3 directionToTarget = (targetPos - CueHierarchy.transform.position).normalized;
        Vector3 desiredRight = directionToTarget;
        Vector3 desiredUp = Vector3.up;
        Vector3 desiredForward = Vector3.Cross(desiredUp, desiredRight).normalized;

        if (desiredForward.sqrMagnitude < 0.001f) desiredForward = Vector3.forward;
        desiredUp = Vector3.Cross(desiredRight, desiredForward).normalized;

        Quaternion baseRotation = Quaternion.LookRotation(desiredForward, desiredUp);
        Quaternion cuePitchRotation = Quaternion.AngleAxis(currentCuePitch, Vector3.forward);

        CueHierarchy.transform.rotation = baseRotation * cuePitchRotation;
    }

    private void UpdateHierarchy()
    {
        Matrix4x4 identity = Matrix4x4.identity;
        CueHierarchy.CompositeXform(ref identity);

        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
             // Debug.Log("=== SceneNode Hierarchy Debug ===");
             // Debug.Log($"Root Position: {CueHierarchy.transform.position}");
             // Debug.Log($"Root Rotation: {CueHierarchy.transform.rotation.eulerAngles}");
             // DebugPrintHierarchy(CueHierarchy, 0);
        }
    }

    public void SetCollidersEnabled(bool enabled)
    {
        if (stickColliders != null)
        {
            foreach (var col in stickColliders)
            {
                if(col != null) col.enabled = enabled;
            }
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180.0f) angle -= 360.0f;
        while (angle < -180.0f) angle += 360.0f;
        return angle;
    }

    // Pass-through helper methods
    public void SetOrbitAngles(float yaw, float pitch) { currentYaw = NormalizeAngle(yaw); currentPitch = Mathf.Clamp(pitch, MinPitchAngle, MaxPitchAngle); }
    public void SetCuePitch(float pitch) { currentCuePitch = Mathf.Clamp(pitch, MinCuePitch, MaxCuePitch); }
    public void ResetAngles() { currentYaw = 0.0f; currentPitch = 20.0f; currentCuePitch = 0.0f; }

    public Vector3 GetCueDirection()
    {
        if (CueBallTarget != null && CueHierarchy != null)
            return (CueBallTarget.position - CueHierarchy.transform.position).normalized;
        return Vector3.forward;
    }
    
    public float GetCurrentYaw() => currentYaw;
    public float GetCurrentPitch() => currentPitch;
    public float GetCurrentCuePitch() => currentCuePitch;

    private void DebugPrintHierarchy(SceneNode node, int depth)
    {
        /* string indent = new string('-', depth * 2);
        Debug.Log($"{indent} SceneNode: {node.name}");
        Debug.Log($"{indent}   NodeOrigin: {node.NodeOrigin}");
        Debug.Log($"{indent}   LocalPos: {node.transform.localPosition}");
        Debug.Log($"{indent}   LocalRot: {node.transform.localRotation.eulerAngles}");

        if (node.PrimitiveList != null)
        {
            Debug.Log($"{indent}   Primitives: {node.PrimitiveList.Count}");
            foreach (var prim in node.PrimitiveList)
            {
                if (prim != null) Debug.Log($"{indent}     - {prim.name}");
            }
        }

        if (node.ChildrenList != null)
        {
            foreach (var child in node.ChildrenList)
            {
                if (child != null) DebugPrintHierarchy(child, depth + 1);
            }
        }
        */
    }
}