
using UnityEngine;
using System.Collections;

public class TempRepairBehaviour : MonoBehaviour
{
    [Header("Temperature System")]
    public TempControlBehaviour temperatureSystem;

    [Header("Repair Save Settings")]
    public string repairId = "Bottom_Temperature_01";

    private bool isRepairing = false;
    private bool isRepaired = false;

    // ------------------------------------
    // START
    // ------------------------------------

    void Start()
    {
        // Check whether the temperature
        // system was previously repaired.
        isRepaired =
            ShipRepairState.IsFixed(repairId);

        if (isRepaired)
        {
            RestorePreviousRepair();
        }
    }

    // ------------------------------------
    // PLAYER INTERACTION
    // ------------------------------------

    private void OnMouseDown()
    {
        if (temperatureSystem == null)
            return;

        if (isRepaired || isRepairing)
            return;

        StartCoroutine(RepairAndSave());
    }

    // ------------------------------------
    // REPAIR AND SAVE
    // ------------------------------------

    private IEnumerator RepairAndSave()
    {
        isRepairing = true;

        // Use the existing temperature
        // control repair system.
        temperatureSystem.RepairSystem();

        // Wait until the repair finishes.
        yield return new WaitForSeconds(
            Mathf.Max(0f, temperatureSystem.repairDelay)
            + 0.05f
        );

        isRepaired = true;
        isRepairing = false;

        // Save the completed repair.
        ShipRepairState.SetFixed(
            repairId,
            true
        );

        Debug.Log(
            "Temperature system repaired and saved!"
        );
    }

    // ------------------------------------
    // RESTORE SAVED REPAIR
    // ------------------------------------

    void RestorePreviousRepair()
    {
        if (temperatureSystem == null)
            return;

        // Reactivate the existing system's
        // repair behavior after scene loading.
        temperatureSystem.RepairSystem();

        Debug.Log(
            "Restoring saved temperature repair: " +
            repairId
        );
    }
}
