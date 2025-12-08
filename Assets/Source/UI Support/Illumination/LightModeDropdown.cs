using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LightModeDropdown : MonoBehaviour
{
    [Header("References")]
    public TMP_Dropdown dropdown;
    public LightControl lightControl; // Reference to your main light script

    [Header("Custom Names (Optional)")]
    [Tooltip("Leave empty to use default Enum names. If filling, must match Enum count (3).")]
    public List<string> customNames; 

    void Start()
    {
        // Safety Checks
        if (lightControl == null)
        {
            Debug.LogError("[LightModeDropdown] LightControl reference is missing");
            return;
        }

        if (dropdown == null)
        {
            Debug.LogError("[LightModeDropdown] Dropdown reference is missing");
            return;
        }

        // 1. Clear existing options
        dropdown.ClearOptions();

        // 2. Populate Dropdown from the LightMode Enum
        string[] enumNames = System.Enum.GetNames(typeof(LightMode));
        List<string> options = new List<string>();

        for (int i = 0; i < enumNames.Length; i++)
        {
            // Use custom name if provided, otherwise use the code name
            if (customNames != null && i < customNames.Count && !string.IsNullOrEmpty(customNames[i]))
            {
                options.Add(customNames[i]);
            }
            else
            {
                // Make the enum name look nicer (e.g. "DirectionalFixed" -> "Directional Fixed")
                options.Add(System.Text.RegularExpressions.Regex.Replace(enumNames[i], "([a-z])([A-Z])", "$1 $2"));
            }
        }

        dropdown.AddOptions(options);

        // 3. Setup Listener
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(OnSelectionChanged);

        // 4. Initialize Visual State
        // We read the current mode from LightControl to set the dropdown correctly at start
        // (This requires LightControl to expose 'currentLightMode' publicly, or we just default to 0)
        dropdown.SetValueWithoutNotify(0); 
    }

    private void OnSelectionChanged(int index)
    {
        // Pass the index directly to your LightControl script
        if (lightControl != null)
        {
            lightControl.SetLightMode(index);
        }
        
        Debug.Log($"[LightModeDropdown] Switched to mode index: {index}");
    }
}