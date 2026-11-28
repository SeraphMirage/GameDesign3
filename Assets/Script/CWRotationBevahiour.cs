
using UnityEngine;
using System.Collections;

public class CWRotationBevahiour : MonoBehaviour
{
    [Header("Repair ID")]
    public string repairId;

    [Header("Rotation Settings")]
    public float rotationAngle = 90f;
    public float duration = 1.5f;
    public Vector3 rotationAxis = Vector3.up;

    private bool isRotating = false;
    private bool isClosed = false;

    private Quaternion closedRotation;

    void Start()
    {
        // Calculate the final rotation.
        closedRotation = transform.localRotation *
            Quaternion.AngleAxis(
                -rotationAngle,
                rotationAxis.normalized
            );

        // Restore saved condition.
        isClosed = ShipRepairState.IsFixed(repairId);

        if (isClosed)
        {
            transform.localRotation = closedRotation;
        }
    }

    public void RotateClockwise()
    {
        if (!isRotating && !isClosed)
        {
            StartCoroutine(RotateRoutine());
        }
    }

    private IEnumerator RotateRoutine()
    {
        isRotating = true;

        Quaternion startRotation =
            transform.localRotation;

        Quaternion endRotation =
            closedRotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(duration, 0.01f)
            );

            transform.localRotation = Quaternion.Slerp(
                startRotation,
                endRotation,
                t
            );

            yield return null;
        }

        transform.localRotation = endRotation;

        isRotating = false;
        isClosed = true;

        // Remember this repair.
        ShipRepairState.SetFixed(repairId, true);
    }
}
