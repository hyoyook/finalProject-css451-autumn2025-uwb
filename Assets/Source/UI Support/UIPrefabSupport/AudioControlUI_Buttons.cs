using UnityEngine;

public partial class AudioControlUI : MonoBehaviour
{
    // handling mute button
    public UnityEngine.UI.Image muteButtonImage;
    public Sprite muteSprite;   // icon when muted
    public Sprite volumeSprite; // icon when unmuted

    #region public button handling
    public void OnMuteButtonClicked()
    {
        Debug.Log("[AudioControlUI] Mute button clicked");
        ToggleMute();
    }

    #endregion
    
    #region private button methods
    private void ToggleMute()
    {
        if (mainMixer == null)
        {
            Debug.LogError("[AudioControl] MainMixer is missing");
            return;
        }

        isMuted = !isMuted;

        if (isMuted)
        {
            // save the curr value 
            if (masterSlider != null)
            {
                lastMasterPercent = masterSlider.GetSliderValue();
            }
            // set the mute in the mixer
            mainMixer.SetFloat(masterParam, k_MutedDb);

            // update button icon
            if (muteButtonImage != null && muteSprite != null)
            {
                muteButtonImage.sprite = muteSprite;
            }
        }
        else
        {
            // restore the last val
            SetMixerVolume(masterParam, lastMasterPercent);

            // unmute in the mixer to the last val
            if (masterSlider != null)
            {
                masterSlider.SetSliderValue(lastMasterPercent);
            }
            if (muteButtonImage != null && muteSprite != null)
            {
                muteButtonImage.sprite = volumeSprite;
            }

        }
        Debug.Log("[AudioControl] Mute toggled: " + isMuted);
    }
    #endregion
}
