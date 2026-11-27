using UnityEngine;
using System.Collections;

public class FlickerBehaviour : MonoBehaviour
{
    [SerializeField] private Light[] targetLights;

    public float minIntensity = 0.9f;
    public float maxIntensity = 2f;
    public float flickerSpeed = 0.1f;

    [Header("Audio")]
    public AudioSource electricalBuzz;
    public AudioSource flickerSound;
    public AudioSource engine;

    private Coroutine flickerRoutine;
    private bool hasTriggered = false;
    private bool powerRestored = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasTriggered || powerRestored) return;

        hasTriggered = true;

        if (flickerRoutine == null)
        {
            foreach (Light light in targetLights)
            {
                if (light != null)
                    light.enabled = true;
            }

            if (engine != null)
            {
                engine.Stop();
            }

            if (electricalBuzz != null)
            {
                electricalBuzz.Stop();
            }

            if (flickerSound != null)
            {
                flickerSound.Play();
            }

            flickerRoutine = StartCoroutine(FlickerLight());
        }
    }

    private IEnumerator FlickerLight()
    {
        while (true)
        {
            foreach (Light light in targetLights)
            {
                if (light != null)
                    light.intensity = Random.Range(
                        minIntensity, maxIntensity);
            }

            yield return new WaitForSeconds(flickerSpeed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!hasTriggered || powerRestored) return;

        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        foreach (Light light in targetLights)
        {
            if (light != null)
                light.enabled = false;
        }

        if (flickerSound != null)
        {
            flickerSound.Stop();
        }
    }

    public void RestorePower()
    {
        powerRestored = true;

        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        foreach (Light light in targetLights)
        {
            if (light != null)
                light.enabled = true;
        }

        if (flickerSound != null)
        {
            flickerSound.Stop();
        }

        if (electricalBuzz != null && !electricalBuzz.isPlaying)
        {
            electricalBuzz.Play();
        }

        if (engine != null && !engine.isPlaying)
        {
            engine.Play();
        }
    }
}
