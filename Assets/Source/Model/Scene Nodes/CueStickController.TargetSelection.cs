using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq; // Needed for sorting hits

public partial class CueStickController
{
    [Header("Target Selection")]
    public string SelectableTag = "Selectable";
    
    // Auto-assigned by the main script, but can be set manually
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

    private void TargetSelectionStart()
    {
        if (RaycastCamera == null) RaycastCamera = Camera.main;
        RefreshSelectableObjects();
    }

    private void UpdateTargetSelection()
    {
        if (Mouse.current == null || RaycastCamera == null) return;

        HandleSelectableCycling();

        if (EnableHoverFeedback) HandleHoverFeedback();

        if (Mouse.current.leftButton.wasPressedThisFrame) HandleTargetClick();
    }

    /// <summary>
    /// Casts a ray that pierces through invisible blockers to find Selectables
    /// </summary>
    private GameObject RaycastForSelectable()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = RaycastCamera.ScreenPointToRay(mousePos);
        
        // GET ALL HITS, not just the first one
        RaycastHit[] hits = Physics.RaycastAll(ray);

        // Sort by distance so we pick the closest Selectable, not one behind a wall
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        foreach (var hit in hits)
        {
            // Ignore the cue stick itself if it gets in the way
            if (hit.transform.root == this.transform.root) continue;

            // Found a valid target? Return it immediately.
            if (hit.collider.CompareTag(SelectableTag))
            {
                return hit.collider.gameObject;
            }
        }

        return null; // Found nothing selectable
    }

    private void HandleHoverFeedback()
    {
        GameObject hitObject = RaycastForSelectable();

        if (hitObject != null)
        {
            if (currentHoveredObject != hitObject)
            {
                ClearHoverEffect();
                currentHoveredObject = hitObject;
                ApplyHoverEffect();
            }
        }
        else
        {
            ClearHoverEffect();
        }
    }

    private void HandleTargetClick()
    {
        GameObject hitObject = RaycastForSelectable();

        if (hitObject != null)
        {
            SetTarget(hitObject.transform);
        }
    }

    // --- VISUALS ---

    private void ApplyHoverEffect()
    {
        if (currentHoveredObject == null) return;

        Renderer renderer = currentHoveredObject.GetComponent<Renderer>();
        if (renderer == null) return;

        originalHoveredMaterials = renderer.materials;
        originalHoveredColors = new Color[originalHoveredMaterials.Length];

        for (int i = 0; i < originalHoveredMaterials.Length; i++)
        {
            if (originalHoveredMaterials[i].HasProperty("_Color"))
            {
                originalHoveredColors[i] = originalHoveredMaterials[i].color;
                originalHoveredMaterials[i].color = HoverTintColor;
            }
        }
    }

    private void ClearHoverEffect()
    {
        if (currentHoveredObject == null) return;

        Renderer renderer = currentHoveredObject.GetComponent<Renderer>();
        if (renderer != null && originalHoveredMaterials != null && originalHoveredColors != null)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length && i < originalHoveredColors.Length; i++)
            {
                if (materials[i].HasProperty("_Color"))
                {
                    materials[i].color = originalHoveredColors[i];
                }
            }
        }

        currentHoveredObject = null;
        originalHoveredMaterials = null;
        originalHoveredColors = null;
    }

    // --- CYCLING LOGIC ---

    private void HandleSelectableCycling()
    {
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            CycleToNextSelectable();
        }
    }

    private void CycleToNextSelectable()
    {
        if (selectableObjects == null || selectableObjects.Length == 0)
        {
            RefreshSelectableObjects();
            return;
        }

        currentSelectableIndex = (currentSelectableIndex + 1) % selectableObjects.Length;
        GameObject selectedObject = selectableObjects[currentSelectableIndex];

        if (selectedObject != null)
        {
            SetTarget(selectedObject.transform);
        }
        else
        {
            RefreshSelectableObjects();
        }
    }

    private void RefreshSelectableObjects()
    {
        selectableObjects = GameObject.FindGameObjectsWithTag(SelectableTag);
        
        currentSelectableIndex = -1;
        if (CueBallTarget != null && selectableObjects.Length > 0)
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

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null) return;
        CueBallTarget = newTarget;
        
        // IMPORTANT: Reset the camera position instantly so it doesn't drift
        // The main script's Update loop will handle the rest
        Debug.Log($"Target switched to: {newTarget.name}");
    }

    private void OnDisable()
    {
        ClearHoverEffect();
    }
}