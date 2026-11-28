
using UnityEngine;

public static class ShipRepairState
{
    private const string Prefix = "ShipRepair_";

    // ------------------------------------
    // CHECK REPAIR STATUS
    // ------------------------------------

    public static bool IsFixed(string repairId)
    {
        if (string.IsNullOrEmpty(repairId))
            return false;

        return PlayerPrefs.GetInt(
            Prefix + repairId, 0
        ) == 1;
    }

    // ------------------------------------
    // SAVE REPAIR STATUS
    // ------------------------------------

    public static void SetFixed(
        string repairId,
        bool fixedStatus)
    {
        if (string.IsNullOrEmpty(repairId))
        {
            Debug.LogWarning(
                "A repair object has no Repair ID!"
            );
            return;
        }

        PlayerPrefs.SetInt(
            Prefix + repairId,
            fixedStatus ? 1 : 0
        );

        PlayerPrefs.Save();

        Debug.Log(
            repairId + " is now " +
            (fixedStatus ? "FIXED" : "BROKEN")
        );
    }

    // ------------------------------------
    // COUNT BROKEN SYSTEMS
    // ------------------------------------

    public static int CountBroken(string[] repairIds)
    {
        if (repairIds == null)
            return 0;

        int brokenCount = 0;

        foreach (string repairId in repairIds)
        {
            if (!string.IsNullOrEmpty(repairId) &&
                !IsFixed(repairId))
            {
                brokenCount++;
            }
        }

        return brokenCount;
    }

    // ------------------------------------
    // RESET ONE REPAIR FOR TESTING
    // ------------------------------------

    public static void ResetRepair(string repairId)
    {
        if (string.IsNullOrEmpty(repairId))
            return;

        PlayerPrefs.DeleteKey(Prefix + repairId);
        PlayerPrefs.Save();
    }
}
