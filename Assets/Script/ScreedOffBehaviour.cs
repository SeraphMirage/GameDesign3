using UnityEngine;

public class ScreedOffBehaviour : MonoBehaviour
{
    public Renderer[] targetObjects;
    public Material ScreenOn;
    public Material ScreenOff;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (Renderer obj in targetObjects)
        {
            if (obj != null)
                obj.material = ScreenOn;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (Renderer obj in targetObjects)
        {
            if (obj != null)
                obj.material = ScreenOff;
        }
    }
}
