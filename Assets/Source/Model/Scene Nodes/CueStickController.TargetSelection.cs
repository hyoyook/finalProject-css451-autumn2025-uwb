using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Partial class for CueStickController - Target Selection functionality
/// 
/// Allows clicking on objects tagged as "Selectable" to change the cue target.
/// Works seamlessly with existing cue controls - just switches what object to hit.
/// 
/// Controls:
/// - Left Click: Select objects tagged as "Selectable" to make them the new target
/// </summary>
public partial class CueStickController
{
    [Header("Target Selection")]
    public string SelectableTag = "Selectable";
    public Camera RaycastCamera;
    public bool EnableHoverFeedback = true;
    public Color HoverTintColor = new Color(1f, 1f, 0.5f, 1f);

    // Track currently hovered object for visual feedback
    private GameObject currentHoveredObject;
    private Material[] originalHoveredMaterials;
    private Color[] originalHoveredColors;

    /// <summary>
    /// Initialize target selection system - call from Start()
    /// </summary>
    private void TargetSelectionStart()
    {
        if (RaycastCamera == null)
        {
            RaycastCamera = Camera.main;
        }
    }

    /// <summary>
    /// Update target selection - call from Update()
    /// </summary>
    private void UpdateTargetSelection()
    {
        if (Mouse.current == null || RaycastCamera == null)
            return;

        // Handle hover feedback
        if (EnableHoverFeedback)
        {
            HandleHoverFeedback();
        }

        // Handle click selection
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleTargetClick();
        }
    }

    /// <summary>
    /// Handles visual feedback when hovering over selectable objects
    /// </summary>
    private void HandleHoverFeedback()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = RaycastCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if we hit a selectable object
            if (hit.collider.CompareTag(SelectableTag))
            {
                // New object hovered
                if (currentHoveredObject != hit.collider.gameObject)
                {
                    // Clear previous hover
                    ClearHoverEffect();

                    // Set new hover
                    currentHoveredObject = hit.collider.gameObject;
                    ApplyHoverEffect();
                }
            }
            else
            {
                // Hit non-selectable object
                ClearHoverEffect();
            }
        }
        else
        {
            // Hit nothing
            ClearHoverEffect();
        }
    }

    /// <summary>
    /// Applies hover visual effect to the current object
    /// </summary>
    private void ApplyHoverEffect()
    {
        if (currentHoveredObject == null)
            return;

        Renderer renderer = currentHoveredObject.GetComponent<Renderer>();
        if (renderer == null)
            return;

        // Store original materials and colors
        originalHoveredMaterials = renderer.materials;
        originalHoveredColors = new Color[originalHoveredMaterials.Length];

        for (int i = 0; i < originalHoveredMaterials.Length; i++)
        {
            originalHoveredColors[i] = originalHoveredMaterials[i].color;
            originalHoveredMaterials[i].color = HoverTintColor;
        }
    }

    /// <summary>
    /// Clears hover visual effect
    /// </summary>
    private void ClearHoverEffect()
    {
        if (currentHoveredObject == null)
            return;

        Renderer renderer = currentHoveredObject.GetComponent<Renderer>();
        if (renderer != null && originalHoveredMaterials != null && originalHoveredColors != null)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length && i < originalHoveredColors.Length; i++)
            {
                materials[i].color = originalHoveredColors[i];
            }
        }

        currentHoveredObject = null;
        originalHoveredMaterials = null;
        originalHoveredColors = null;
    }

    /// <summary>
    /// Handles clicking on objects to select them as the new target
    /// </summary>
    private void HandleTargetClick()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = RaycastCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit object has the selectable tag
            if (hit.collider.CompareTag(SelectableTag))
            {
                SetTarget(hit.transform);
            }
        }
    }

    /// <summary>
    /// Sets a new target for the cue stick to aim at
    /// </summary>
    /// <param name="newTarget">The transform of the new target object</param>
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning("CueStickController: Attempted to set null target");
            return;
        }

        CueBallTarget = newTarget;
        Debug.Log($"Cue target switched to: {newTarget.name}");

        // Optional: Reset orbit angles when switching targets
        // Uncomment if you want the cue to reset to a default angle each time
        // ResetAngles();
    }

    /// <summary>
    /// Gets the current target transform
    /// </summary>
    public Transform GetTarget()
    {
        return CueBallTarget;
    }

    /// <summary>
    /// Clean up hover effects when disabled
    /// </summary>
    private void OnDisable()
    {
        ClearHoverEffect();
    }
}
