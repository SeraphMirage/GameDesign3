using UnityEngine;
using System.Collections;

public class CWRotationBevahiour : MonoBehaviour
{
    public float rotationAngle = 90f;
    public float duration = 1.5f;

    // Choose the rotation axis in the Inspector
    public Vector3 rotationAxis = Vector3.up;

    private bool isRotating = false;
    private bool isClosed = false;

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

        Quaternion startRotation = transform.localRotation;

        Quaternion endRotation = startRotation *
            Quaternion.AngleAxis(-rotationAngle, rotationAxis.normalized);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

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
    }

}
