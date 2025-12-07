using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public GameObject rootPanel;        // Root UI Panel
    public GameObject settingMenu;      // SettingMenu
    public GameObject audioControl;     // AudioControl
    public GameObject textureControl;   // TextureControl
    public GameObject lightControl;     // LightControl
    public GameObject informationPage;  // InformationPage

    public Button resetButton;          // Scene Reset
    public Button closeGameButton;      // Close the game
    public Button closeUI;              // Close UI, return to the gameplay

    public Button resetButton_2ndPanel; // restButton in 2nd Panel

    private void Start()
    {
        // everything is hidden
        rootPanel.SetActive(false);
        ShowOnly(settingMenu);
    }

    private void Update()
    {
        // [ESC] opens / closes setting
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!rootPanel.activeSelf)
            {
                Debug.Log("[UIManager] ESC key pressed. Opening Settings");
                OpenSettings();
            }
            else
            {
                Debug.Log("[UIManager] ESC key pressed. Closing Settings");
                CloseSettings();
            }
        }
    }

    #region Open and Closing Setting
    private void OpenSettings()
    {
        rootPanel.SetActive(true);
        ShowOnly(settingMenu);

    }
    private void CloseSettings()
    {
        rootPanel.SetActive(false);
    }

    // back arrow from each sub page calls this
    public void BackToSettingMenu() 
    {
        ShowOnly(settingMenu);
    }
    #endregion

    #region Buttons on Setting Menus (public)
    public void OpenAudioControl() 
    { 
        ShowOnly(audioControl); 
    }
    public void OpenTextureControl() 
    {
        ShowOnly(textureControl);
    }
    public void OpenLightControl() 
    {
        ShowOnly(lightControl);
    }
    public void OpenInformationPage() 
    {
        ShowOnly(informationPage);
    }

    public void ResetScene() 
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    // source: https://discussions.unity.com/t/how-do-i-create-a-exit-quit-button/142125
    public void CloseGame() 
    { 
        Application.Quit();
    }

    // back arrow: return to game
    public void ReturnToGame() 
    {
        CloseSettings();
    }
    #endregion

    #region private helpers 
    private void ShowOnly(GameObject page) 
    {
        settingMenu.SetActive(false);
        audioControl.SetActive(false);
        textureControl.SetActive(false);
        lightControl.SetActive(false);
        informationPage.SetActive(false);

        page.SetActive(true);
    }

    #endregion



}
