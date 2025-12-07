using UnityEngine;

/// <summary>
/// Detects when any object goes out of bounds (falls below a certain Y level)
/// and returns it to its original position.
/// Works for balls, chalk, cues, or any physics object that can fall off the table.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ObjectOutOfBounds : MonoBehaviour
{
    [Header("Out of Bounds Detection")]
    public float OutOfBoundsY = 0.5f;

    [Header("Return Settings")]
    public bool FreezeOnReturn = true;

    [Header("Visual Feedback")]
    public bool FlashOnReturn = true;
    public float FlashDuration = 0.5f;
    public Color FlashColor = Color.red;

    // Stored original position/rotation at start
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Rigidbody objectRb;
    private Renderer objectRenderer;
    private Material objectMaterial;
    private Color originalColor;
    private bool isFlashing = false;

    void Start()
    {
        // Store original transform
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Get components
        objectRb = GetComponent<Rigidbody>();
        objectRenderer = GetComponent<Renderer>();
        
        if (objectRenderer != null)
        {
            objectMaterial = objectRenderer.material;
            originalColor = objectMaterial.color;
        }

        Debug.Log($"[ObjectOutOfBounds] {gameObject.name} monitoring position. Out of bounds below Y = {OutOfBoundsY}");
    }

    void Update()
    {
        // Check if object has fallen below the out-of-bounds threshold
        if (transform.position.y < OutOfBoundsY)
        {
            ReturnObjectToTable();
        }
    }

    /// <summary>
    /// Returns the object to its original position
    /// </summary>
    public void ReturnObjectToTable()
    {
        // Move the object back to its original position and rotation
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Stop all movement
        if (FreezeOnReturn && objectRb != null)
        {
            objectRb.linearVelocity = Vector3.zero;
            objectRb.angularVelocity = Vector3.zero;
        }

        // Visual feedback
        if (FlashOnReturn && !isFlashing)
        {
            StartCoroutine(FlashObject());
        }

        Debug.Log($"[ObjectOutOfBounds] {gameObject.name} returned to original position at {originalPosition}");
    }

    /// <summary>
    /// Flash the object to indicate it was returned
    /// </summary>
    private System.Collections.IEnumerator FlashObject()
    {
        if (objectMaterial == null) yield break;

        isFlashing = true;
        float elapsed = 0f;

        while (elapsed < FlashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 4f, 1f); // Flash 4 times per second
            objectMaterial.color = Color.Lerp(originalColor, FlashColor, t);
            yield return null;
        }

        // Restore original color
        objectMaterial.color = originalColor;
        isFlashing = false;
    }

    /// <summary>
    /// Update the stored original position (useful if object moves during gameplay)
    /// </summary>
    public void UpdateOriginalPosition()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    /// <summary>
    /// Check if the object is currently out of bounds
    /// </summary>
    public bool IsOutOfBounds()
    {
        return transform.position.y < OutOfBoundsY;
    }

    // Gizmos for visualization in Scene view
    private void OnDrawGizmosSelected()
    {
        // Draw the return position (current position in edit mode, original position in play mode)
        Gizmos.color = Color.green;
        Vector3 returnPos = Application.isPlaying ? originalPosition : transform.position;
        Gizmos.DrawWireSphere(returnPos, 0.15f);
        Gizmos.DrawLine(returnPos + Vector3.up * 0.5f, returnPos - Vector3.up * 0.5f);

        // Draw the out-of-bounds plane
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
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

        // Draw original position marker in play mode
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(originalPosition, 0.1f);
        }
    }
}
