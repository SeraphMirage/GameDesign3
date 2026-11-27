using TMPro;
using UnityEngine;
using System.Collections;

public class TempControlBehaviour : MonoBehaviour
{
    public TMP_Text temperatureText;
    public TMP_Text statusText;
    public float repairDelay = 3f;
    private bool isRepairing = false;

    [Header("Audio")]
    public AudioSource repairSound;

    private bool isRepaired = false;

    void Start()
    {
        UpdateDisplay();
    }

    public void RepairSystem()
    {
        if (isRepaired || isRepairing)
            return;

        StartCoroutine(RepairRoutine());
    }

    private IEnumerator RepairRoutine()
    {
        isRepairing = true;

        if (repairSound != null)
        {
            repairSound.Play();
        }

        yield return new WaitForSeconds(repairDelay);

        isRepaired = true;
        UpdateDisplay();

        isRepairing = false;
    }

    void UpdateDisplay()
    {
        if (isRepaired)
        {
            temperatureText.text = "2°C";
            statusText.text = "SYSTEM ONLINE";
        }
        else
        {
            temperatureText.text = "+18°C";
            statusText.text = "SYSTEM FAILURE";
        }
    }
}
