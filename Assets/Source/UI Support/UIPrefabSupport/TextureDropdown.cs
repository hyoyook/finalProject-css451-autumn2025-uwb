using UnityEngine;
using TMPro;

public class TextureDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public MeshRenderer targetRenderer;  // the quad renderer
    public Material[]   materials;

    void Start()
    {
        if (targetRenderer == null)
        {
            Debug.LogError("[TextureDropdown] Target MeshRenderer reference is missing");
            return;
        }

        if (dropdown == null)
        {
            Debug.LogError("[TextureDropdown] Dropdown reference is missing");
            return;
        }

        dropdown.options.Clear();       
        foreach (var mat in materials)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(mat.name));
        }

        dropdown.onValueChanged.AddListener(OnSelectionChanged);

        // initialize
        if (materials.Length > 0)
        {
            OnSelectionChanged(0);
        }
    }

    private void OnSelectionChanged(int index)
    {
        if (index < 0 || index >= materials.Length || materials[index] == null)
        {
            Debug.LogError($"[TextureDropdown] Material at index {index} is NULL");
            return;
        }
        targetRenderer.material = materials[index];

        Debug.Log($"[TextureDropdown] Cloth material changed to: {targetRenderer.material.name})");

    }
}
