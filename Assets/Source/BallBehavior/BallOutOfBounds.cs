using UnityEngine;

/// <summary>
/// Detects when a ball goes out of bounds (falls below a certain Y level)
/// and returns it to a safe position on the table.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BallOutOfBounds : MonoBehaviour
{
    [Header("Out of Bounds Detection")]
    public float OutOfBoundsY = 0.5f; // Below this Y, ball is out of bounds

    [Header("Return Settings")]
    public Vector3 ReturnPosition = new Vector3(0f, 2.147f, 2f); // Default: head spot

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
