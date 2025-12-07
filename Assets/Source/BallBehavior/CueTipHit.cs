using UnityEngine;

public class CueTipHit : MonoBehaviour
{
    [Header("Settings")]
    public float forceMultiplier;
    public float minHitSpeed = 5.0f;      // New: Minimum speed to ensure ball moves
    public string targetTag = "CueBall";

    private Vector3 lastPosition;
    private Vector3 currentVelocity;
    private float lastHitTime = 0f;

    void Start()
    {
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 displacement = transform.position - lastPosition;
        currentVelocity = displacement / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < lastHitTime + 0.5f) return;

        if (other.CompareTag(targetTag))
        {
            Rigidbody ballRb = other.GetComponent<Rigidbody>();

            if (ballRb != null)
            {
                lastHitTime = Time.time;

                // 1. Get current speed
                float hitSpeed = currentVelocity.magnitude;

                // 2. ENFORCE MINIMUM SPEED
                // If the math says speed is 0.1, we pretend it's 5.0 so the ball definitely moves
                if (hitSpeed < minHitSpeed) hitSpeed = minHitSpeed;

                Vector3 hitDirection = transform.forward;
                
                // 3. Apply Force
                ballRb.AddForce(hitDirection * hitSpeed * forceMultiplier, ForceMode.Impulse);

                Debug.Log($"HIT! Speed used: {hitSpeed} | Multiplier: {forceMultiplier}");
            }
        }
    }
}