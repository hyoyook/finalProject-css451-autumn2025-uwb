using UnityEngine;

/// <summary>
/// Detects when a ball goes out of bounds (falls below a certain Y level)
/// and returns it to a safe position on the table.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BallOutOfBounds : MonoBehaviour
{
    [Header("Out of Bounds Detection")]
    [Tooltip("Below this Y coordinate, ball is out of bounds")]
    public float OutOfBoundsY = 0.5f;

    [Tooltip("If ball touches any of these colliders, it returns to the top of the table (optional)")]
    public Collider TriggerCollider1;
    public Collider TriggerCollider2;
    public Collider TriggerCollider3;
    public Collider TriggerCollider4;
    public Collider TriggerCollider5;
    public Collider TriggerCollider6;

    [Header("Return Settings")]
    [Tooltip("Position to return the ball to (X and Z only if using table, or absolute position)")]
    public Vector3 ReturnPosition = new Vector3(0f, 2.147f, 2f); // Default: head spot

    [Tooltip("Reference to the table for calculating return height")]
    public Transform Table;

    public float HeightAboveTable = 0.15f; // Half the ball diameter plus a bit

    public bool FreezeOnReturn = true;

    [Header("Visual Feedback")]
    public bool FlashOnReturn = true;
    public float FlashDuration = 0.5f;

    private Rigidbody ballRb;
    private Renderer ballRenderer;
    private Material ballMaterial;
    private Color originalColor;
    private bool isFlashing = false;

    void Start()
    {
        ballRb = GetComponent<Rigidbody>();
        ballRenderer = GetComponent<Renderer>();
        
        if (ballRenderer != null)
        {
            ballMaterial = ballRenderer.material;
            originalColor = ballMaterial.color;
        }

        Debug.Log($"[BallOutOfBounds] Monitoring ball position. Out of bounds below Y = {OutOfBoundsY}");
    }

    void Update()
    {
        // Check if ball has fallen below the out-of-bounds threshold
        if (transform.position.y < OutOfBoundsY)
        {
            ReturnBallToTable();
        }
    }

    /// <summary>
    /// Detect collision with the trigger colliders
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // Check if we hit any of the specified trigger colliders
        if (IsTriggerCollider(collision.collider))
        {
            Debug.Log($"[BallOutOfBounds] Ball touched trigger collider: {collision.collider.name}");
            ReturnBallToTable();
        }
    }

    /// <summary>
    /// Detect trigger collision (if the collider is set as trigger)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Check if we entered any of the specified trigger colliders
        if (IsTriggerCollider(other))
        {
            Debug.Log($"[BallOutOfBounds] Ball entered trigger collider: {other.name}");
            ReturnBallToTable();
        }
    }

    /// <summary>
    /// Helper method to check if a collider is one of our trigger colliders
    /// </summary>
    private bool IsTriggerCollider(Collider collider)
    {
        return (TriggerCollider1 != null && collider == TriggerCollider1) ||
               (TriggerCollider2 != null && collider == TriggerCollider2) ||
               (TriggerCollider3 != null && collider == TriggerCollider3) ||
               (TriggerCollider4 != null && collider == TriggerCollider4) ||
               (TriggerCollider5 != null && collider == TriggerCollider5) ||
               (TriggerCollider6 != null && collider == TriggerCollider6);
    }

    /// <summary>
    /// Returns the ball to the table at the specified position
    /// </summary>
    public void ReturnBallToTable()
    {
        // Calculate return position
        Vector3 returnPos = CalculateReturnPosition();

        // Move the ball
        transform.position = returnPos;

        // Stop all movement
        if (FreezeOnReturn && ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }

        // Visual feedback
        if (FlashOnReturn && !isFlashing)
        {
            StartCoroutine(FlashBall());
        }

        Debug.Log($"[BallOutOfBounds] Ball returned to table at {returnPos}");
    }

    /// <summary>
    /// Calculates where to return the ball based on settings
    /// </summary>
    private Vector3 CalculateReturnPosition()
    {
        // If table reference is set, place relative to table
        if (Table != null)
        {
            // Get table's top surface position
            float tableY = Table.position.y;
            
            // If table has a collider, get its top bound
            Collider tableCollider = Table.GetComponent<Collider>();
            if (tableCollider != null)
            {
                tableY = tableCollider.bounds.max.y;
            }

            // Return position with correct height
            return new Vector3(
                ReturnPosition.x,
                tableY + HeightAboveTable,
                ReturnPosition.z
            );
        }

        // Otherwise use absolute position
        return ReturnPosition;
    }

    /// <summary>
    /// Flash the ball to indicate it was returned
    /// </summary>
    private System.Collections.IEnumerator FlashBall()
    {
        if (ballMaterial == null) yield break;

        isFlashing = true;
        Color flashColor = Color.red;
        float elapsed = 0f;

        while (elapsed < FlashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 4f, 1f); // Flash 4 times per second
            ballMaterial.color = Color.Lerp(originalColor, flashColor, t);
            yield return null;
        }

        // Restore original color
        ballMaterial.color = originalColor;
        isFlashing = false;
    }

    /// <summary>
    /// Manually set the return position (useful for in-hand placement)
    /// </summary>
    public void SetReturnPosition(Vector3 newPosition)
    {
        ReturnPosition = newPosition;
    }

    /// <summary>
    /// Check if the ball is currently out of bounds
    /// </summary>
    public bool IsOutOfBounds()
    {
        return transform.position.y < OutOfBoundsY;
    }

    // Gizmos for visualization in Scene view
    private void OnDrawGizmosSelected()
    {
        // Draw the return position
        Gizmos.color = Color.green;
        Vector3 returnPos = CalculateReturnPosition();
        Gizmos.DrawWireSphere(returnPos, 0.15f);
        Gizmos.DrawLine(returnPos + Vector3.up * 0.5f, returnPos - Vector3.up * 0.5f);

        // Draw the out-of-bounds plane
        Gizmos.color = Color.red;
        float gridSize = 10f;
        Vector3 center = new Vector3(0, OutOfBoundsY, 0);
        
        // Draw a grid at the out-of-bounds Y level
        for (int i = -5; i <= 5; i++)
        {
            Gizmos.DrawLine(
                center + new Vector3(-gridSize, 0, i * 2),
                center + new Vector3(gridSize, 0, i * 2)
            );
            Gizmos.DrawLine(
                center + new Vector3(i * 2, 0, -gridSize),
                center + new Vector3(i * 2, 0, gridSize)
            );
        }

        // Draw line to table if assigned
        if (Table != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, Table.position);
        }
    }
}
