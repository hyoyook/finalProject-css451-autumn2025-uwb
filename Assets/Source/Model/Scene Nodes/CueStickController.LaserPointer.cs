using UnityEngine;

/// <summary>
/// Partial class for CueStickController - Laser Pointer functionality
/// Shows a red sphere on the cue ball where the cue is aiming
/// </summary>
public partial class CueStickController
{
    [Header("Laser Pointer")]
    [Tooltip("The red sphere that shows where the cue will hit")]
    public GameObject LaserPointer;
    
    [Tooltip("The transform at the tip of the cue stick (where ray originates)")]
    public Transform CueTip;
    
    [Tooltip("Show debug line in Scene view")]
    public bool ShowLaserDebugLine = true;
    
    [Tooltip("Color of the debug line")]
    public Color LaserLineColor = Color.red;

    /// <summary>
    /// Initialize the laser pointer - call this from Start()
    /// </summary>
    private void LaserPointerStart()
    {
        if (LaserPointer == null)
        {
            Debug.Log("Laser Pointer is missing!");
        }
    }


    /// <summary>
    /// Updates the laser pointer position based on where the cue is aiming.
    /// Call this from Update() after updating cue position.
    /// </summary>
    private void UpdateLaserPointer()
    {
        if (LaserPointer == null || CueBallTarget == null || CueHierarchy == null)
            return;

        // Get cue tip position - use CueTip if assigned, otherwise estimate from hierarchy
        Vector3 cuePosition;
        Vector3 cueDirection;
        
        if (CueTip != null)
        {
            cuePosition = CueTip.position;
            // Use the cue's actual axis direction (negative X bc blender import weird)
            cueDirection = -CueTip.right;
        }
        else
        {
            // Fallback: use hierarchy position and forward
            cuePosition = CueHierarchy.transform.position;
            cueDirection = CueHierarchy.transform.forward;
        }

        // Draw debug line in Scene view
        if (ShowLaserDebugLine)
        {
            Debug.DrawRay(cuePosition, cueDirection * OrbitDistance * 2f, LaserLineColor);
        }

        // Try raycast first if ball has a collider
        Collider ballCollider = CueBallTarget.GetComponent<Collider>();
        if (ballCollider != null)
        {
            Ray ray = new Ray(cuePosition, cueDirection);
            RaycastHit hit;

            if (ballCollider.Raycast(ray, out hit, OrbitDistance * 2f))
            {
                // We hit the ball - show laser pointer at hit point
                LaserPointer.SetActive(true);
                LaserPointer.transform.position = hit.point + hit.normal * 0.01f;
                
                // Draw line to hit point in green
                if (ShowLaserDebugLine)
                {
                    Debug.DrawLine(cuePosition, hit.point, Color.green);
                }
                return;
            }
        }

        // Fallback: Use sphere intersection math if no collider or miss
        CalculateLaserPointerMath(cuePosition, cueDirection);
    }

    /// <summary>
    /// Calculate laser pointer position using sphere intersection math
    /// </summary>
    private void CalculateLaserPointerMath(Vector3 cuePosition, Vector3 cueDirection)
    {
        Vector3 ballCenter = CueBallTarget.position;
        float ballRadius = CueBallTarget.localScale.x * 0.5f; // Assuming uniform scale

        // Vector from ray origin to sphere center
        Vector3 oc = cuePosition - ballCenter;
        
        // Quadratic formula coefficients for ray-sphere intersection
        // Ray: P(t) = cuePosition + t * cueDirection
        // Sphere: |P - ballCenter|^2 = ballRadius^2
        float a = Vector3.Dot(cueDirection, cueDirection); // Should be 1 if normalized
        float b = 2.0f * Vector3.Dot(oc, cueDirection);
        float c = Vector3.Dot(oc, oc) - ballRadius * ballRadius;
        
        float discriminant = b * b - 4 * a * c;
        
        // Draw debug info
        if (ShowLaserDebugLine)
        {
            Debug.DrawLine(cuePosition, ballCenter, Color.cyan); // Line to ball center
        }
        
        if (discriminant >= 0)
        {
            // Ray intersects sphere - find nearest hit point
            float t = (-b - Mathf.Sqrt(discriminant)) / (2.0f * a);
            
            if (t > 0) // Hit is in front of ray origin
            {
                Vector3 hitPoint = cuePosition + cueDirection * t;
                
                LaserPointer.SetActive(true);
                LaserPointer.transform.position = hitPoint;
                
                // Draw hit confirmation
                if (ShowLaserDebugLine)
                {
                    Debug.DrawLine(cuePosition, hitPoint, Color.green);
                }
                return;
            }
        }

        // Not aiming at ball or ball is behind
        LaserPointer.SetActive(false);
    }

    /// <summary>
    /// Show or hide the laser pointer
    /// </summary>
    public void SetLaserPointerVisible(bool visible)
    {
        if (LaserPointer != null)
        {
            LaserPointer.SetActive(visible);
        }
    }

}
