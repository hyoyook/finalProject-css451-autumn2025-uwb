using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BGMDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public Transform playlist;
    
    private AudioSource[] mTracks;
    private int mCurrIndex = -1;
    private bool mIsPaused = false;     // for play/pause button

    // source: https://discussions.unity.com/t/how-do-you-find-an-inactive-game-object/859182/2
    private void Awake()
    {
        if (playlist == null)
        {
            Debug.LogError("[BGMDropdown] Playlist is not assigned");
            return;
        }

        // find all AudioSource in the playlist
        mTracks = playlist.GetComponentsInChildren<AudioSource>(includeInactive: true);
        if (mTracks == null || mTracks.Length == 0)
        {
            Debug.LogError("[BGMPlaylistUI] No AudioSource found in the playlist");
            return;
        }

        // build dropdown options
        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var src in mTracks)
        {
            string label;
            if (src != null && src.clip != null)
            {
                label = src.clip.name;
            }
            else
            {
                label = src.gameObject.name;
            }
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(options);

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

        // stop previous if any
        if (mCurrIndex >= 0 && mCurrIndex < mTracks.Length)
        {
            var prev = mTracks[mCurrIndex];
            if (prev != null)
                prev.Stop();
        }

        // stop all others just to be safe
        for (int i = 0; i < mTracks.Length; i++)
        {
            if (i == index) continue;
            if (mTracks[i] != null)
                mTracks[i].Stop();
        }

        // play new one
        var srcNew = mTracks[index];
        if (srcNew != null)
            srcNew.Play();

        mCurrIndex = index;

        // update text to "Currently Playing: XYZ"
        if (dropdown != null && dropdown.captionText != null)
        {
            string label = dropdown.options[index].text;
            dropdown.captionText.text = $"{label}";
        }

        // keep dropdown value in sync without retriggering onValueChanged
        if (dropdown != null && dropdown.value != index)
        {
            dropdown.SetValueWithoutNotify(index);
        }

        Debug.Log($"[BGMPlaylistUI] Now playing track #{index}");
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

        var src = mTracks[mCurrIndex];
        if (src == null)
        {
            return;
        }

        // pause curr playing track
        if (src.isPlaying)
        {
            src.Pause();
            mIsPaused = true;
        }
        else
        {
            // resume if it was paused, otherwise just play
            if (mIsPaused)
            {
                src.UnPause();
            }
            else
            {
                src.Play();
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
        Debug.Log("[BGMDropdown] prev button clicked");
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
        Debug.Log("[BGMDropdown] Stop button clicked");
        if (mTracks == null || mTracks.Length == 0)
        {
            Debug.Log("[BGMDropdown] No track to play");
            return;
        }

        if (mCurrIndex < 0 || mCurrIndex >= mTracks.Length)
        {
            return;
        }

        var src = mTracks[mCurrIndex];
        if (src != null)
        {
            src.Stop();
        }

        mIsPaused = false;
    }
    #endregion

}


