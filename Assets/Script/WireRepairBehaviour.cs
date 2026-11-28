
using UnityEngine;
using System.Collections;

public class WireRepairBehaviour : MonoBehaviour
{
    [Header("Repair ID")]
    public string repairId;

    [Header("Repair Settings")]
    public Transform repairedPosition;
    public float repairSpeed = 3f;

    private bool isRepaired = false;
    private bool isRepairing = false;

    void Start()
    {
        // Check previous repair state.
        isRepaired = ShipRepairState.IsFixed(repairId);

        // Restore the repaired position.
        if (isRepaired && repairedPosition != null)
        {
            transform.SetPositionAndRotation(
                repairedPosition.position,
                repairedPosition.rotation
            );
        }
    }

    public void RepairWire()
    {
        if (isRepaired ||
            isRepairing ||
            repairedPosition == null)
            return;

        StartCoroutine(RepairRoutine());
    }

    private IEnumerator RepairRoutine()
    {
        isRepairing = true;

        Vector3 startPosition =
            transform.position;

        Quaternion startRotation =
            transform.rotation;

        Vector3 targetPosition =
            repairedPosition.position;

        Quaternion targetRotation =
            repairedPosition.rotation;

        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * repairSpeed;

            float t = Mathf.Clamp01(progress);

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            transform.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        transform.SetPositionAndRotation(
            targetPosition,
            targetRotation
        );

        isRepaired = true;
        isRepairing = false;

        // Remember this wire was repaired.
        ShipRepairState.SetFixed(repairId, true);
    }
}
