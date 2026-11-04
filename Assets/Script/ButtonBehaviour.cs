using UnityEngine;
using UnityEngine.Events;

public class ButtonBehaviour : MonoBehaviour
{
    public Material redMaterial;
    public Material greenMaterial;

    public FlickerBehaviour[] powerZones;
    public ScreedOffBehaviour[] screenZones;
    public GameObject[] objectsToActivate;

    private Renderer objectRenderer;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null && redMaterial != null)
        {
            objectRenderer.material = redMaterial;
        }
    }

    public void PressButton()
    {
        if (objectRenderer == null ||
            redMaterial == null ||
            greenMaterial == null)
            return;

        if (objectRenderer.sharedMaterial == greenMaterial)
        {
            objectRenderer.material = redMaterial;
        }
        else
        {
            objectRenderer.material = greenMaterial;

            foreach (FlickerBehaviour zone in powerZones)
            {
                if (zone != null)
                    zone.RestorePower();
            }

            foreach (ScreedOffBehaviour zone in screenZones)
            {
                if (zone != null)
                    zone.TurnScreensOn();
            }

            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }
}
