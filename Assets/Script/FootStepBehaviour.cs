using UnityEngine;

public class FootStepBehaviour : MonoBehaviour
{
    public Rigidbody playerRigidbody;
    public AudioSource footstepSource;

    public AudioClip[] footstepSounds;

    public float stepInterval = 0.45f;
    public float minimumSpeed = 0.2f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    private float stepTimer;

    private void Update()
    {
        if (playerRigidbody == null ||
            footstepSource == null ||
            footstepSounds.Length == 0)
            return;

        Vector3 velocity = playerRigidbody.linearVelocity;
        velocity.y = 0f;

        if (velocity.magnitude > minimumSpeed)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        int randomIndex = Random.Range(
            0, footstepSounds.Length);

        footstepSource.pitch = Random.Range(
            minPitch, maxPitch);

        footstepSource.PlayOneShot(
            footstepSounds[randomIndex]);
    }
}
