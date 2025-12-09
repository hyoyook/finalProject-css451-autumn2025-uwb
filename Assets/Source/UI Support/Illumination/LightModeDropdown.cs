using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LightModeDropdown : MonoBehaviour
{
    public LightControl lightControl;
    public TMP_Dropdown dropdown;

    // label to display
    public string dayLabel       = "Day";
    public string nightLabel     = "Night";
    public string spotlightLabel = "Spotlight";

    // dictionary: index-> name
    private Dictionary<int, string> modeNames;

    private bool _initializing = false;

    private void Awake()
    {
        if (lightControl == null)
        {
            Debug.LogError("[LightModeDropdown] LightControl reference is missing");
            enabled = false;
            return;
        }

        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();

        if (dropdown == null)
        {
            Debug.LogError("[LightModeDropdown] TMP_Dropdown not found");
            enabled = false;
            return;
        }

        // dictionary
        modeNames = new Dictionary<int, string>()
        {
            { 0, dayLabel },
            { 1, nightLabel },
            { 2, spotlightLabel }
        };

        // setup
        _initializing = true;

        dropdown.ClearOptions();
        dropdown.options.Add(new TMP_Dropdown.OptionData(dayLabel));
        dropdown.options.Add(new TMP_Dropdown.OptionData(nightLabel));
        dropdown.options.Add(new TMP_Dropdown.OptionData(spotlightLabel));

        dropdown.SetValueWithoutNotify(0);

        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        _initializing = false;

        // set initial mode
        OnDropdownChanged(dropdown.value);

        Debug.Log("[LightModeDropdown] Initialized!");
    }

    public void OnDropdownChanged(int index)
    {
        if (_initializing) return;
        if (lightControl == null) return;

        if (modeNames.TryGetValue(index, out string modeName))
        {
            Debug.Log($"[LightModeDropdown] Mode changed to ({index}) {modeName}");
        }
        else
        {
            Debug.Log($"[LightModeDropdown] Mode index {index} (Unknown)");
        }

        // toss it to LightControl
        lightControl.SetLightMode(index);
    }
}
