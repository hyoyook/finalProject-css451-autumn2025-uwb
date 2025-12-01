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

        // Get the direction the cue is pointing
        // The cue points along positive X (right) axis based on our rotation setup
        Vector3 cueDirection = CueHierarchy.transform.forward;
        Vector3 cuePosition = CueHierarchy.transform.position;

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
        Debug.Log("CalculateLaserPointerMath called");
        Vector3 toTarget = CueBallTarget.position - cuePosition;
        float ballRadius = CueBallTarget.localScale.x * 0.5f; // Assuming uniform scale

        // Project cue direction onto line to ball center
        float projection = Vector3.Dot(toTarget, cueDirection);
        // Debug.Log("Laser Projection: " + projection);
        if (projection > 0) // Pointing towards ball
        {
            Debug.Log("Laser is pointing towards the ball");
            // Find closest point on ray to ball center
            Vector3 closestPoint = cuePosition + cueDirection * projection;
            float distToCenter = Vector3.Distance(closestPoint, CueBallTarget.position);

            if (distToCenter <= ballRadius)
            {
                Debug.Log("Laser intersects sphere");
                // Ray intersects sphere - calculate hit point
                float hitDistance = projection - Mathf.Sqrt(ballRadius * ballRadius - distToCenter * distToCenter);
                Vector3 hitPoint = cuePosition + cueDirection * hitDistance;

                LaserPointer.SetActive(true);
                LaserPointer.transform.position = hitPoint;
                return;
            }
        }

        // Not aiming at ball
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
