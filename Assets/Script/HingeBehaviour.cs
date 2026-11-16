using UnityEngine;
using System.Collections;

public class HingeBehaviour : MonoBehaviour
{
    public Transform pivotPoint;
    public Vector3 rotationAxis = Vector3.up;

    public float rotationAngle = 90f;
    public float speed = 90f;

    private bool isOpen = false;
    private bool isRotating = false;

    public void RotateHinge()
    {
        if (isRotating || pivotPoint == null)
            return;

        StartCoroutine(RotateRoutine());
    }

    private IEnumerator RotateRoutine()
    {
        isRotating = true;

        float targetAngle = isOpen
            ? -rotationAngle
            : rotationAngle;

        float rotatedAmount = 0f;

        while (rotatedAmount < rotationAngle)
        {
            float step = Mathf.Min(
                speed * Time.deltaTime,
                rotationAngle - rotatedAmount
            );

            transform.RotateAround(
                pivotPoint.position,
                rotationAxis.normalized,
                step * Mathf.Sign(targetAngle)
            );

            rotatedAmount += step;

            yield return null;
        }

        isOpen = !isOpen;
        isRotating = false;
    }
}
