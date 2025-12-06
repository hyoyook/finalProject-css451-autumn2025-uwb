using UnityEngine;

public class BallSound : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip ballHitSound;
    public float minImpactForce = 0.02f; // ignore tiny collisions eg. with tablecloth
    public float maxVolumeForce = 20f;  // maxing here so it does not blast 


    // source: https://learn.unity.com/course/beginning-audio-in-unity/tutorial/sound-effects-scripting-1?version=2019.4#5f4f75b6edbc2a034289e2f6
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        float volume = Mathf.Clamp01(impact / maxVolumeForce);
        Debug.Log($"[BallSound] Collision with {collision.collider.name}, tag={collision.collider.tag}, impact={impact}, volume={volume}");

        if (impact < minImpactForce)
        {
            Debug.Log("[BallSound] Impact too small, ignoring");
            return; // if too light of a touch, ignore
        }

        if (collision.collider.CompareTag("Tag 3 Balls"))
        {
            Debug.Log("[BallSound] Ball ↔ Ball collision!");
        }
        if (collision.collider.CompareTag("CueStick"))
        {
            Debug.Log("[BallSound] Cue ↔ Ball collision!");
        }

        // either balls hit other balls or cuestick hit the cue ball
        if (collision.collider.CompareTag("Tag 3 Balls") || collision.collider.CompareTag("CueStick")) 
        {
            Debug.Log("[BallSound] Playing sound");
            audioSource.PlayOneShot(ballHitSound, volume);
        }

    }

}
