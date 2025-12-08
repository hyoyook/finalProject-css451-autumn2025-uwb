using System.Collections.Generic;
using TMPro;
using UnityEngine.Audio;
using UnityEngine;

public class BGMDropdown : MonoBehaviour
{
    public TMP_Dropdown     dropdown;
    public AudioMixerGroup  bgmGroup; 

    private AudioClip[]     mTracks;
    private AudioSource     mSource;

    private int mCurrIndex = -1;
    private bool mIsPaused = false;     // for play/pause button

    private void Awake()
    {
        if (dropdown == null)
        {
            Debug.LogError("[BGMDropdown] dropdown is not assigned");
            return;
        }
        
        mTracks = Resources.LoadAll<AudioClip>("Audio/Playlist");

        if (mTracks == null || mTracks.Length == 0)
        {
            Debug.LogError("[BGMDropdown] No AudioClips found in Resources/Audio/Playlist/");
            return;
        }

        // audioSource on this GameObject
        mSource = gameObject.AddComponent<AudioSource>();
        mSource.playOnAwake = false;
        mSource.loop = false;

        // output audio
        if (bgmGroup != null)
        {
            mSource.outputAudioMixerGroup = bgmGroup;
        }
        else 
        {
            Debug.LogError("[BGMDropdown] No OutputAudioMixerGroup(bgmGroup) assigned");
        }

        // Build dropdown options from clip names
        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var track in mTracks)
        {
            string label = (track != null) ? track.name : "(Missing Clip)";
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        // play the first track
        PlayTrack(0);
    }

    private void OnDropdownChanged(int index)
    {
        PlayTrack(index);
    }

    // ChatGPT: "play one track at a time, update texts to show curr playing track title" 
    private void PlayTrack(int index)
    {
        if (mTracks == null || mTracks.Length == 0)
        {
            Debug.Log("[BGMDropdown] Nothing to play");
            return;
        }

        index = Mathf.Clamp(index, 0, mTracks.Length - 1);

        AudioClip clip = mTracks[index];
        if (clip == null)
        {
            Debug.LogWarning($"[BGMDropdown] Clip at index {index} is NULL");
            return;
        }

        // stop curr if any is playing
        if (mSource.isPlaying)
        {
            mSource.Stop();
        }

        mSource.clip = clip;
        mSource.Play();

        mCurrIndex = index;
        mIsPaused = false;

        // update text
        if (dropdown != null && dropdown.captionText != null)
        {
            string label = dropdown.options[index].text;
            dropdown.captionText.text = label;
        }

        // keep dropdown value in sync without retriggering onValueChanged
        if (dropdown != null && dropdown.value != index)
        {
            dropdown.SetValueWithoutNotify(index);
        }

        Debug.Log($"[BGMDropdown] Now playing track #{index}: {clip.name}");
    }

    #region Play/Pause/Next/Prev
    public void OnPlayPauseClicked() 
    {
        if (mTracks == null || mTracks.Length == 0)
        {
            Debug.Log("[BGMDropdown] No track to play");
            return;
        }

        // nothing has been selected yet, start at 0
        if (mCurrIndex < 0 || mCurrIndex >= mTracks.Length)
        {
            PlayTrack(0); 
            return;
        }

        if (mSource == null || mSource.clip == null)
        { 
            return; 
        }

        // pause curr playing track
        if (mSource.isPlaying)
        {
            mSource.Pause();
            mIsPaused = true;
        }
        else
        {
            // resume if it was paused, otherwise just play
            if (mIsPaused)
            {
                mSource.UnPause();
            }
            else
            {
                mSource.Play();
            }

            mIsPaused = false;
        }
    }

    public void OnNextClicked()
    {
        if (mTracks == null || mTracks.Length == 0)
        {
            Debug.Log("[BGMDropdown] No track to play");
            return;
        }
   
        int nextIndex;
        if (mCurrIndex < 0) 
        {
            // nothing is playing, play the first track
            nextIndex = 0;
        }
        else
        {
            nextIndex = (mCurrIndex + 1) % mTracks.Length;  // if last track, then play first track
        }

        PlayTrack(nextIndex);
    }

    public void OnPrevClicked()
    {
        if (mTracks == null || mTracks.Length == 0)
        {
            Debug.Log("[BGMDropdown] No track to play");
            return;
        }

        int prevIndex;
        if (mCurrIndex < 0)
        {
            // nothing is playing, play the first track
            prevIndex = 0;
        }
        else
        {
            prevIndex = (mCurrIndex - 1 + mTracks.Length) % mTracks.Length; // if first track, play last track
        }

        PlayTrack(prevIndex);
    }

    public void OnStopClicked()
    {
        if (mTracks == null || mTracks.Length == 0)
        {
            Debug.Log("[BGMDropdown] No track to play");
            return;
        }

        if (mCurrIndex < 0 || mCurrIndex >= mTracks.Length)
        {
            return;
        }

        mSource.Stop();
        mIsPaused = false;
    }
    #endregion

}


