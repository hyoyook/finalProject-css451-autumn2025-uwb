using UnityEngine;
using TMPro;

public class TextureDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public MeshRenderer targetRenderer;  // the quad renderer
    public Material[] materials;

    void Start()
    {
        dropdown.options.Clear();
        foreach (var mat in materials)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(mat.name));
        }

        dropdown.onValueChanged.AddListener(OnSelectionChanged);

        // initialize
        if (materials.Length > 0)
            OnSelectionChanged(0);
    }

    void OnSelectionChanged(int index)
    {
        if (index < 0 || index >= materials.Length)
        {
            return;
        }
        targetRenderer.material = materials[index];
    }
}
