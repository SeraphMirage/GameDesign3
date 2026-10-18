using UnityEngine;

public class FlickerBehaviour : MonoBehaviour
{
    private Light light;

    public float minIntensity = .9f;
    public float maxIntensity = 2f;
    public float flickerSpeed = 0.1f;

    private void Start()
    {
        light = GetComponent<Light>();

        InvokeRepeating("Flicker", 0f, flickerSpeed);
    }

    private void Flicker()
    {
        float randomIntensity = Random.Range(minIntensity, maxIntensity);
        light.intensity = randomIntensity;
    }
}
