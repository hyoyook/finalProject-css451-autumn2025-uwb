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
/// - C Key: Cycle through all selectable objects
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

    // Selectable objects cycling
    private GameObject[] selectableObjects;
    private int currentSelectableIndex = -1;

    /// <summary>
    /// Initialize target selection system - call from Start()
    /// </summary>
    private void TargetSelectionStart()
    {
        if (RaycastCamera == null)
        {
            RaycastCamera = Camera.main;
        }

        // Find all selectable objects in the scene
        RefreshSelectableObjects();
    }

    /// <summary>
    /// Update target selection - call from Update()
    /// </summary>
    private void UpdateTargetSelection()
    {
        if (Mouse.current == null || RaycastCamera == null)
            return;

        // Handle cycling through selectable objects with "C" key
        HandleSelectableCycling();

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
    /// Handles cycling through selectable objects when "C" is pressed
    /// </summary>
    private void HandleSelectableCycling()
    {
        if (Keyboard.current == null)
            return;

        // Check if C key was pressed this frame
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            CycleToNextSelectable();
        }
    }

    /// <summary>
    /// Cycles to the next selectable object in the list
    /// </summary>
    private void CycleToNextSelectable()
    {
        if (selectableObjects == null || selectableObjects.Length == 0)
        {
            Debug.LogWarning("[CueStickController] No selectable objects found. Make sure objects are tagged as 'Selectable'.");
            RefreshSelectableObjects();
            return;
        }

        // Move to next index (wrap around)
        currentSelectableIndex = (currentSelectableIndex + 1) % selectableObjects.Length;

        // Get the selected object
        GameObject selectedObject = selectableObjects[currentSelectableIndex];

        if (selectedObject != null)
        {
            // Update the cue ball target to the new selection
            SetTarget(selectedObject.transform);
            Debug.Log($"[CueStickController] Cycled to selectable object: {selectedObject.name} ({currentSelectableIndex + 1}/{selectableObjects.Length})");
        }
        else
        {
            Debug.LogWarning($"[CueStickController] Selectable object at index {currentSelectableIndex} is null. Refreshing list.");
            RefreshSelectableObjects();
        }
    }

    /// <summary>
    /// Refreshes the list of selectable objects from the scene
    /// </summary>
    private void RefreshSelectableObjects()
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(SelectableTag);
        selectableObjects = foundObjects;
        
        if (selectableObjects.Length > 0)
        {
            // Debug.Log($"[CueStickController] Found {selectableObjects.Length} selectable objects:");
            for (int i = 0; i < selectableObjects.Length; i++)
            {
                // Debug.Log($"  [{i}] {selectableObjects[i].name}");
            }
            
            // Set current index to the object that matches CueBallTarget, or start at -1
            currentSelectableIndex = -1;
            if (CueBallTarget != null)
            {
                for (int i = 0; i < selectableObjects.Length; i++)
                {
                    if (selectableObjects[i].transform == CueBallTarget)
                    {
                        currentSelectableIndex = i;
                        break;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"[CueStickController] No objects with '{SelectableTag}' tag found in the scene.");
            currentSelectableIndex = -1;
        }
    }

    /// <summary>
    /// Gets the current selectable object
    /// </summary>
    public GameObject GetCurrentSelectable()
    {
        if (selectableObjects != null && currentSelectableIndex >= 0 && currentSelectableIndex < selectableObjects.Length)
        {
            return selectableObjects[currentSelectableIndex];
        }
        return null;
    }

    /// <summary>
    /// Gets the total number of selectable objects
    /// </summary>
    public int GetSelectableCount()
    {
        return selectableObjects != null ? selectableObjects.Length : 0;
    }

    /// <summary>
    /// Gets the current selectable index (0-based)
    /// </summary>
    public int GetCurrentSelectableIndex()
    {
        return currentSelectableIndex;
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
