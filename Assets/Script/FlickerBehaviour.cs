using UnityEngine;
using System.Collections;

public class FlickerBehaviour : MonoBehaviour
{
    [SerializeField] private Light targetLight;

    public float minIntensity = 0.9f;
    public float maxIntensity = 2f;
    public float flickerSpeed = 0.1f;

    private Coroutine flickerRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (flickerRoutine == null)
        {
            targetLight.enabled = true;
            flickerRoutine = StartCoroutine(FlickerLight());
        }
    }

    private IEnumerator FlickerLight()
    {
        while (true)
        {
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);

            yield return new WaitForSeconds(flickerSpeed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        if (targetLight != null)
        {
            targetLight.enabled = false;
        }
    }
}
