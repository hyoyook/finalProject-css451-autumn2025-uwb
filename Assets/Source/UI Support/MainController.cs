using UnityEngine;
using UnityEngine.InputSystem;
public class MainController : MonoBehaviour
{
    public AudioControlUI audioControl;
    public UIManager settingsUI;

    public Key muteKey = Key.M;
    public Key infoKey = Key.I;

    private void Start()
    {
        
    }
    private void Update()
    {
        var ky = Keyboard.current;
        if (ky == null)
        {
            return;
        }

        // Global mute
        if (ky[muteKey].wasPressedThisFrame)
            audioControl.OnMuteKeyGlobal();

        // Global open Information page
        if (ky[infoKey].wasPressedThisFrame)
            settingsUI.OpenInformationPage();
    }
}
