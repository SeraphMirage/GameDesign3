using UnityEngine;
using TMPro;

public class ScreedOffBehaviour : MonoBehaviour
{
    public Renderer[] targetObjects;
    public Material ScreenOn;
    public Material ScreenOff;

    public TMP_Text[] targetTexts;

    private bool hasTriggered = false;
    private bool powerRestored = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasTriggered) return;

        hasTriggered = true;

        foreach (Renderer obj in targetObjects)
        {
            if (obj != null)
                obj.material = ScreenOn;
        }

        foreach (TMP_Text text in targetTexts)
        {
            if (text != null)
                text.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!hasTriggered) return;
        if (powerRestored) return;

        foreach (Renderer obj in targetObjects)
        {
            if (obj != null)
                obj.material = ScreenOff;
        }

        foreach (TMP_Text text in targetTexts)
        {
            if (text != null)
                text.gameObject.SetActive(false);
        }
    }

    public void TurnScreensOn()
    {
        powerRestored = true;

        foreach (Renderer obj in targetObjects)
        {
            if (obj != null)
                obj.material = ScreenOn;
        }

        foreach (TMP_Text text in targetTexts)
        {
            if (text != null)
                text.gameObject.SetActive(true);
        }
    }
}
