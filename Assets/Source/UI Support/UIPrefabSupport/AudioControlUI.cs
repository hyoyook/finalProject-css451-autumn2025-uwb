using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public partial class AudioControlUI : MonoBehaviour
{
    public AudioMixer mainMixer;
    public string masterParam = "MasterVolume";
    public string musicParam  = "BGMVolume";
    public string sfxParam    = "EffectsVolume";

    public SliderWithEcho masterSlider;         // MasterSlider object
    public SliderWithEcho musicSlider;          // BGMSlider object
    public SliderWithEcho sfxSlider;            // EffectSlider object

    public Key muteKey = Key.M;
    private bool isMuted = false;
    private float lastMasterPercent = 80f;      // default: 50%

    private const float k_MinLinear = 0.0001f;  // avoid log10(0), bad for mathing
    private const float k_MutedDb   = -80f;     // typical "off" value

    private void Awake()
    {
        Debug.Assert(mainMixer != null, "[AudioControlUI] mainMixer is not assigned!");

        // initial setup
        if (masterSlider != null)
        {
            masterSlider.SetSliderLabel("Master Volume");
            masterSlider.InitSliderRange(0f, 100f, 50f);    // default 50%
            masterSlider.SetSliderListener(OnMasterChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.SetSliderLabel("Music Volume");
            musicSlider.InitSliderRange(0f, 100f, 40f);
            musicSlider.SetSliderListener(OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetSliderLabel("Sound Effect");
            sfxSlider.InitSliderRange(0f, 100f, 60f);
            sfxSlider.SetSliderListener(OnSfxChanged);
        }

        // apply to mixer
        ApplyAllSlidersToMixer();
    }

    #region sliders 
    // source: https://discussions.unity.com/t/change-audio-mixer-volume-with-slider/920323/4
    private void OnMasterChanged(float v)
    {
        // If slider moves while muted → unmute first
        if (isMuted)
        {
            isMuted = false;               // clear mute state
            lastMasterPercent = v;   // update baseline
            SetMixerVolume(masterParam, v);
            UpdateMuteIcon();
            // Debug.Log("[AudioControlUI] Slider changed -> auto unmuted.");
            return;
        }

        // Normal behavior (not muted)
        lastMasterPercent = v;
        SetMixerVolume(masterParam, v);
    }

    private void OnMusicChanged(float v)
    {
        SetMixerVolume(musicParam, v);
    }

    private void OnSfxChanged(float v)
    {
        SetMixerVolume(sfxParam, v);
    }

    #endregion

    #region mixer helpers

    // source: https://discussions.unity.com/t/change-audio-mixer-volume-with-slider/920323/4
    private void SetMixerVolume(string param, float v) 
    {
        if (mainMixer == null || string.IsNullOrEmpty(param)) 
        {
            Debug.LogError("[AudioControl] MainMixer or Exposed Parmeter is missing");
            return;
        }

        // percentage to linear between 0 and 1 
        float linear = Mathf.Clamp(v / 100f, k_MinLinear, 1f);
        // linear to dB
        float dB = Mathf.Log10(linear) * 20f;
        mainMixer.SetFloat(param, dB);

    }
    private void ApplyAllSlidersToMixer() 
    {
        if (masterSlider != null)
            OnMasterChanged(masterSlider.GetSliderValue());

        if (musicSlider != null)
            OnMusicChanged(musicSlider.GetSliderValue());

        if (sfxSlider != null)
            OnSfxChanged(sfxSlider.GetSliderValue());
    }

    #endregion
}
