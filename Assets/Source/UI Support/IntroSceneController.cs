using UnityEngine;
using UnityEngine.Playables;       // PlayableDirector
using UnityEngine.SceneManagement; // SceneManager

public class IntroSceneController : MonoBehaviour
{
    public PlayableDirector introTimeline;

    // for fading transition
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    public string gameScene; // what we are transitioning to

    // source:
    // https://discussions.unity.com/t/what-and-how-is-the-best-way-to-fade-in-out-when-loading-switching-scenes/906519
    // https://discussions.unity.com/t/simple-image-fade-in-script/837653
    // https://discussions.unity.com/t/mostly-future-proof-loading-screen-for-everything-coroutines/882151/2
    // register callback when Timeline finishes
    // source: https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Playables.PlayableDirector-stopped.html
    private void Start()
    {
        if (introTimeline == null)
        {
            introTimeline = GetComponent<PlayableDirector>();
        }

        // fade starts transparent
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
        }

        introTimeline.stopped += OnIntroFinished;
    }

    // source: https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Playables.PlayableDirector-stopped.html
    private void OnIntroFinished(PlayableDirector director)
    {
        // when intro timeline finishes, start smooth transition
        StartCoroutine(FadeAndLoad());
    }

    // ChatGPT: Unity fading scene transition using coroutine
    private System.Collections.IEnumerator FadeAndLoad()
    {
        // fade from transparent (0) to black (1)
        if (fadeGroup != null && fadeDuration > 0f)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / fadeDuration);
                fadeGroup.alpha = normalized;
                yield return null;
            }

            fadeGroup.alpha = 1f;
        }

        // load next scene
        if (!string.IsNullOrEmpty(gameScene))
        {
            SceneManager.LoadScene(gameScene);
        }
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextIndex);
        }
    }
}
