using TMPro;
using UnityEngine;

public class TempControlBehaviour : MonoBehaviour
{
    public TMP_Text temperatureText;
    public TMP_Text statusText;

    public Material brokenMaterial;
    public Material workingMaterial;

    public Renderer indicatorLight;

    private bool isRepaired = false;

    void Start()
    {
        UpdateDisplay();
    }

    public void RepairSystem()
    {
        if (isRepaired) return;

        isRepaired = true;

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (isRepaired)
        {
            temperatureText.text = "-18°C";
            statusText.text = "SYSTEM ONLINE";

            if (indicatorLight != null)
                indicatorLight.material = workingMaterial;
        }
        else
        {
            temperatureText.text = "+18°C";
            statusText.text = "SYSTEM FAILURE";

            if (indicatorLight != null)
                indicatorLight.material = brokenMaterial;
        }
    }
}
