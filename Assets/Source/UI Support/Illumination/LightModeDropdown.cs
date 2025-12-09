using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LightModeDropdown : MonoBehaviour
{
    [Header("References")]
    public TMP_Dropdown dropdown;
    public LightControl lightControl; 

    void Start()
    {
        // 1. Debugging References
        if (dropdown == null)
        {
            Debug.LogError("❌ [LightModeDropdown] STOP! You forgot to assign the 'Dropdown' in the Inspector.");
            return;
        }
        if (lightControl == null)
        {
            Debug.LogError("❌ [LightModeDropdown] STOP! You forgot to assign 'Light Control' (GameManager) in the Inspector.");
            return;
        }

        Debug.Log("✅ [LightModeDropdown] Setup started...");

        // 2. Clear and Build List
        dropdown.ClearOptions();
        List<string> options = new List<string> { "Day", "Night", "Spotlight" };
        dropdown.AddOptions(options);
        Debug.Log("✅ [LightModeDropdown] Options added: Day, Night, Spotlight");

        // 3. Connect Listener
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(OnSelectionChanged);

        // 4. Force Initial State (Default to Night/1 to verify change)
        dropdown.SetValueWithoutNotify(1); 
        OnSelectionChanged(1); 
    }

    private void OnSelectionChanged(int index)
    {
        Debug.Log($"👉 [LightModeDropdown] Player selected option: {index}");
        
        if (lightControl != null)
        {
            lightControl.SetLightMode(index);
            Debug.Log($"✅ [LightModeDropdown] Sent command to LightControl!");
        }
    }
}