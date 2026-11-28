
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    [Header("Repair ID")]
    public string repairId;

    [Header("Materials")]
    public Material redMaterial;
    public Material greenMaterial;

    [Header("Systems")]
    public FlickerBehaviour[] powerZones;
    public ScreedOffBehaviour[] screenZones;
    public GameObject[] objectsToActivate;

    private Renderer objectRenderer;
    private bool isFixed;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        isFixed = ShipRepairState.IsFixed(repairId);

        UpdateAppearance();

        if (isFixed)
        {
            RestoreSystems();
        }
    }

    public void PressButton()
    {
        if (isFixed)
            return;

        isFixed = true;

        UpdateAppearance();
        RestoreSystems();

        ShipRepairState.SetFixed(repairId, true);
    }

    void UpdateAppearance()
    {
        if (objectRenderer == null)
            return;

        if (isFixed && greenMaterial != null)
        {
            objectRenderer.material = greenMaterial;
        }
        else if (!isFixed && redMaterial != null)
        {
            objectRenderer.material = redMaterial;
        }
    }

    void RestoreSystems()
    {
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
