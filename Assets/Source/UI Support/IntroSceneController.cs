/*
 * References
* [1] Unity Discussions, “What and how is the best way to fade in/out when loading/switching scenes?” 
*     [Online]. Available: https://discussions.unity.com/t/what-and-how-is-the-best-way-to-fade-in-out-when-loading-switching-scenes/906519
* [2] Unity Discussions, “Simple image fade in script.” 
*     [Online]. Available: https://discussions.unity.com/t/simple-image-fade-in-script/837653
* [3] Unity Discussions, “Mostly future-proof loading screen for everything (coroutines).” 
*     [Online]. Available: https://discussions.unity.com/t/mostly-future-proof-loading-screen-for-everything-coroutines/882151/2
* [4] Unity Technologies, “PlayableDirector.stopped,” Unity Scripting API. 
*     [Online]. Available: https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Playables.PlayableDirector-stopped.html
* [5] OpenAI, LLC., “Unity fading scene transition using coroutine.” ChatGPT, Accessed: Dec. 8, 2025.
*     [Online]. Available: https://chat.openai.com
*/

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

    // source: Unity discussions on scene transition with fading [1][2][3]
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

    // source: unity technologies [4]
    private void OnIntroFinished(PlayableDirector director)
    {
        // when intro timeline finishes, start smooth transition
        StartCoroutine(FadeAndLoad());
    }

    // source: ChatGPT, Unity fading scene transition using coroutine [5]
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
