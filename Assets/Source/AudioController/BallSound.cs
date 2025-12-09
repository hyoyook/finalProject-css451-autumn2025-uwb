using UnityEngine;

public class BallSound : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip ballHitSound;
    public float minImpactForce = 0.01f; // ignore tiny collisions eg. with tablecloth
    public float maxVolumeForce = 20f;  // maxing here so it does not blast 


    // source: https://learn.unity.com/course/beginning-audio-in-unity/tutorial/sound-effects-scripting-1?version=2019.4#5f4f75b6edbc2a034289e2f6
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        // Debug.Log($"[BallSound] Raw impact with {collision.collider.name}, impact={impact}");

        if (impact < minImpactForce)
        {
            // Debug.Log($"[BallSound] Impact too small ({impact}), ignoring");
            return; // if too light of a touch, ignore
        }
        const float maxImpactForLoud = 3f;

        // ChatGPT: almost inaudible sfx. Reasonable range setting based on collision impact
        // map impact between 0 and 1
        float t = Mathf.InverseLerp(minImpactForce, maxImpactForLoud, impact);
        t = Mathf.Clamp01(t);
        
        // minimum audible volume (25%) and a curve
        float minAudible = 0.25f;
        float volume = Mathf.Lerp(minAudible, 1f, Mathf.Pow(t, 0.7f));

        // either balls hit other balls or cuestick hit the cue ball
        if (collision.collider.CompareTag("Balls") || collision.collider.CompareTag("CueStick")) 
        {
            // Debug.Log("[BallSound] Playing sound");
            audioSource.PlayOneShot(ballHitSound, volume);
        }

    }

}
