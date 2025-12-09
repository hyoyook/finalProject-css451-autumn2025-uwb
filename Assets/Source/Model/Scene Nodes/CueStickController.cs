using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the cue stick orbiting around a target (cue ball).
/// Includes Camera Switching (FPS vs Overhead) and "Ghost Stick" logic.
/// </summary>
public partial class CueStickController : MonoBehaviour
{
    [Header("Scene Node References")]
    [Tooltip("The root SceneNode that will orbit around the target")]
    public SceneNode CueHierarchy;

    [Tooltip("The target transform to orbit around (cue ball position)")]
    public Transform CueBallTarget;

    [Header("Camera System")]
    [Tooltip("The static top-down camera for watching the table")]
    public Camera MainOverheadCamera;
    [Tooltip("The First-Person camera attached to the Cue Stick")]
    public Camera CueFpsCamera;

    [Header("Orbit Settings")]
    [Tooltip("Distance from the cue ball to orbit at")]
    public float OrbitDistance = 5.0f;

    public float YawSpeed = 60.0f;
    public float PitchSpeed = 40.0f;
    public float MinPitchAngle = 5.0f;
    public float MaxPitchAngle = 60.0f;

    [Header("Cue Pitch Settings (CTRL + WASD)")]
    public float CuePitchSpeed = 30.0f;
    public float MinCuePitch = -20.0f;
    public float MaxCuePitch = 45.0f;
    public float CueYawSpeed = 30.0f;
    public float MinCueYaw = -45.0f;
    public float MaxCueYaw = 45.0f;

    // Current orbit angles
    private float currentYaw = 0.0f;
    private float currentPitch = 20.0f;

    // Current cue stick pitch/yaw (in-place rotation)
    private float currentCuePitch = 0.0f;
    private float currentCueYaw = 0.0f;

    // References to disable physics
    private Collider[] stickColliders;
    private Rigidbody stickRb;
    
    // Reference to visuals (Renderers) so we can hide them without killing the Camera child
    private Renderer[] stickVisuals;

    // Track the current camera state (Default to false = Overhead view)
    private bool isFpsMode = false;

    private void Start()
    {
        // SAFETY 1: Get all colliders to disable them during aiming
        if (CueHierarchy != null)
        {
            stickColliders = CueHierarchy.GetComponentsInChildren<Collider>();
            stickRb = CueHierarchy.GetComponent<Rigidbody>();
            
            // NEW: Get all renderers to hide/show the stick visually
            stickVisuals = CueHierarchy.GetComponentsInChildren<Renderer>();

            // SAFETY 2: Make stick kinematic so gravity doesn't affect it
            if (stickRb != null) stickRb.isKinematic = true; 
        }

        // Initialize position based on current settings
        if (CueBallTarget != null && CueHierarchy != null)
        {
            UpdateCuePosition();
        }

        HierarchyStart();
        LaserPointerStart();
        ShotStart();
        TargetSelectionStart();
    }

    private void Update()
    {
        if (CueBallTarget == null || CueHierarchy == null)
            return;
        
        UpdateTargetSelection();
        
        // SAFETY 3: Force colliders OFF every frame while aiming
        SetCollidersEnabled(false);

        bool ctrlHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
        if (ctrlHeld)
        {
            HandleCuePitchInput();
        }
        else
        {
            HandleOrbitInput();
        }

        // Core Update Logic (Camera switching happens here)
        UpdateCuePosition();

        UpdateHierarchy();
        UpdateNodeSelection();
        UpdateLaserPointer();
        UpdateShot();
    }

    private void HandleOrbitInput()
    {
        if (Keyboard.current == null) return;

        float yawInput = 0.0f;
        float pitchInput = 0.0f;

        if (Keyboard.current.aKey.isPressed) yawInput = -1.0f;
        else if (Keyboard.current.dKey.isPressed) yawInput = 1.0f;

        if (Keyboard.current.wKey.isPressed) pitchInput = 1.0f;
        else if (Keyboard.current.sKey.isPressed) pitchInput = -1.0f;

        currentYaw += yawInput * YawSpeed * Time.deltaTime;
        currentYaw = NormalizeAngle(currentYaw);

        currentPitch += pitchInput * PitchSpeed * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, MinPitchAngle, MaxPitchAngle);
    }

    private void HandleCuePitchInput()
    {
        if (Keyboard.current == null) return;

        float pitchInput = 0.0f;
        float yawInput = 0.0f;

        if (Keyboard.current.wKey.isPressed) pitchInput = 1.0f;
        else if (Keyboard.current.sKey.isPressed) pitchInput = -1.0f;

        if (Keyboard.current.aKey.isPressed) yawInput = -1.0f;
        else if (Keyboard.current.dKey.isPressed) yawInput = 1.0f;

        currentCuePitch += pitchInput * CuePitchSpeed * Time.deltaTime;
        currentCuePitch = Mathf.Clamp(currentCuePitch, MinCuePitch, MaxCuePitch);

        currentCueYaw += yawInput * CueYawSpeed * Time.deltaTime;
        currentCueYaw = Mathf.Clamp(currentCueYaw, MinCueYaw, MaxCueYaw);
    }

    private void UpdateCuePosition()
    {
        Rigidbody ballRb = CueBallTarget.GetComponent<Rigidbody>();
        
        // Check if ball is rolling
        bool isBallMoving = ballRb != null && (!ballRb.isKinematic && ballRb.linearVelocity.magnitude > 0.01f);

        if (isBallMoving)
        {
            // STATE: Ball is Rolling (Action Phase)
            
            // 1. Force state to Overhead (so when ball stops, we are still in Overhead until player switches)
            isFpsMode = false;

            // 2. Hide stick visuals
            SetStickVisuals(false);

            // 3. Force Camera to Overhead
            SetCameraMode(false); 

            // 4. Stop tracking
            return; 
        }
        else
        {
            // STATE: Ball is Stopped (Aiming Phase)

            // 1. Listen for "V" key to toggle view
            if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
            {
                isFpsMode = !isFpsMode; // Toggle between True (FPS) and False (Overhead)
            }

            // 2. Apply the chosen camera mode
            SetCameraMode(isFpsMode);

            // 3. Show stick visuals (Visible in BOTH views while aiming)
            if (!CueHierarchy.gameObject.activeSelf) CueHierarchy.gameObject.SetActive(true);
            SetStickVisuals(true);
        }

        // --- Standard Orbit Code (Runs in BOTH views while aiming) ---
        // This allows you to aim/rotate the stick even while looking from the Overhead view!
        
        float yawRad = currentYaw * Mathf.Deg2Rad;
        float pitchRad = currentPitch * Mathf.Deg2Rad;

        float x = OrbitDistance * Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
        float y = OrbitDistance * Mathf.Sin(pitchRad);
        float z = OrbitDistance * Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);

        Vector3 offset = new Vector3(x, y, z);
        Vector3 targetPos = CueBallTarget.position;

        CueHierarchy.transform.position = targetPos + offset;

        Vector3 directionToTarget = (targetPos - CueHierarchy.transform.position).normalized;

        Vector3 desiredRight = directionToTarget;
        Vector3 desiredUp = Vector3.up;
        Vector3 desiredForward = Vector3.Cross(desiredUp, desiredRight).normalized;

        if (desiredForward.sqrMagnitude < 0.001f) desiredForward = Vector3.forward;
        desiredUp = Vector3.Cross(desiredRight, desiredForward).normalized;

        Quaternion baseRotation = Quaternion.LookRotation(desiredForward, desiredUp);
        CueHierarchy.transform.rotation = baseRotation;

        SceneNode handNode = GetDeepestNode();
        if (handNode != null)
        {
            Quaternion cueYawRotation = Quaternion.AngleAxis(currentCueYaw, Vector3.up);
            Quaternion cuePitchRotation = Quaternion.AngleAxis(currentCuePitch, Vector3.forward);
            handNode.transform.localRotation = cueYawRotation * cuePitchRotation;
        }
    }

    private void UpdateHierarchy()
    {
        Matrix4x4 identity = Matrix4x4.identity;
        CueHierarchy.CompositeXform(ref identity);

        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            Debug.Log("=== SceneNode Hierarchy Debug ===");
            DebugPrintHierarchy(CueHierarchy, 0);
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

    // --- NEW HELPER METHODS ---

    /// <summary>
    /// Shows or hides the stick visuals without disabling the GameObject (keeps Camera alive)
    /// </summary>
    public void SetStickVisuals(bool visible)
    {
        if (stickVisuals != null)
        {
            foreach (var r in stickVisuals)
            {
                if (r != null) r.enabled = visible;
            }
        }
    }

    /// <summary>
    /// Switches between Overhead (false) and Cue FPS (true) cameras
    /// AND updates the RaycastCamera so clicking works in both views.
    /// </summary>
    private void SetCameraMode(bool useCueCamera)
    {
        // 1. Enable/Disable the actual cameras
        if (MainOverheadCamera != null) MainOverheadCamera.enabled = !useCueCamera;
        if (CueFpsCamera != null)       CueFpsCamera.enabled = useCueCamera;

        // 2. CRITICAL: Tell the TargetSelection script which camera to use for clicking
        // If we are in FPS mode, we must raycast from the FPS camera.
        // If we are in Overhead mode, we must raycast from the Overhead camera.
        RaycastCamera = useCueCamera ? CueFpsCamera : MainOverheadCamera;
    }

    // --- UTILITIES ---

    private void DebugPrintHierarchy(SceneNode node, int depth)
    {
        // ... (Existing debug code omitted for brevity, logic unchanged) ...
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180.0f) angle -= 360.0f;
        while (angle < -180.0f) angle += 360.0f;
        return angle;
    }

    public void SetOrbitAngles(float yaw, float pitch) {
        currentYaw = NormalizeAngle(yaw);
        currentPitch = Mathf.Clamp(pitch, MinPitchAngle, MaxPitchAngle);
    }
    public void SetCuePitch(float pitch) { currentCuePitch = Mathf.Clamp(pitch, MinCuePitch, MaxCuePitch); }
    public void SetCueYaw(float yaw) { currentCueYaw = Mathf.Clamp(yaw, MinCueYaw, MaxCueYaw); }
    public void ResetAngles() { currentYaw = 0.0f; currentPitch = 20.0f; currentCuePitch = 0.0f; currentCueYaw = 0.0f; }

    public Vector3 GetCueDirection()
    {
        if (CueBallTarget != null && CueHierarchy != null)
            return (CueBallTarget.position - CueHierarchy.transform.position).normalized;
        return Vector3.forward;
    }

    public float GetCurrentYaw() => currentYaw;
    public float GetCurrentPitch() => currentPitch;
    public float GetCurrentCuePitch() => currentCuePitch;
    public float GetCurrentCueYaw() => currentCueYaw;
}