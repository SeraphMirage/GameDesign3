using UnityEngine;
using System.Collections;

public class ComputerInteractionBehaviour : MonoBehaviour
{
    //Player ref
    public Camera playerCamera;
    public MonoBehaviour playerController;
    public MonoBehaviour cameraLookScript;
    public Rigidbody playerRigidbody;

    //Computer view
    public Transform computerViewPoint;
    public float moveDuration = 1f;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private RigidbodyConstraints originalConstraints;
    private CursorLockMode originalCursorLock;
    private bool originalCursorVisible;

    private bool isInteracting = false;
    private bool isTransitioning = false;

    private void Update()
    {
        if (isInteracting && !isTransitioning &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(ExitComputer());
        }
    }

    public void EnterComputer()
    {
        if (isInteracting) return;

        if (playerCamera == null ||
            playerController == null ||
            cameraLookScript == null ||
            playerRigidbody == null ||
            computerViewPoint == null)
        {
            Debug.LogWarning("Computer interaction references are missing!");
            return;
        }

        isInteracting = true;
        isTransitioning = true;

        originalCameraPosition = playerCamera.transform.position;
        originalCameraRotation = playerCamera.transform.rotation;

        originalConstraints = playerRigidbody.constraints;

        originalCursorLock = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        playerController.enabled = false;
        cameraLookScript.enabled = false;

        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(MoveCameraToComputer());
    }

    private IEnumerator MoveCameraToComputer()
    {
        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / moveDuration);

            playerCamera.transform.position = Vector3.Lerp(
                startPosition,
                computerViewPoint.position,
                progress
            );

            playerCamera.transform.rotation = Quaternion.Slerp(
                startRotation,
                computerViewPoint.rotation,
                progress
            );

            yield return null;
        }

        playerCamera.transform.position = computerViewPoint.position;
        playerCamera.transform.rotation = computerViewPoint.rotation;

        isTransitioning = false;
    }

    private IEnumerator ExitComputer()
    {
        isTransitioning = true;

        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / moveDuration);

            playerCamera.transform.position = Vector3.Lerp(
                startPosition,
                originalCameraPosition,
                progress
            );

            playerCamera.transform.rotation = Quaternion.Slerp(
                startRotation,
                originalCameraRotation,
                progress
            );

            yield return null;
        }

        playerCamera.transform.position = originalCameraPosition;
        playerCamera.transform.rotation = originalCameraRotation;

        playerRigidbody.constraints = originalConstraints;
        playerRigidbody.linearVelocity = Vector3.zero;

        playerController.enabled = true;
        cameraLookScript.enabled = true;

        Cursor.lockState = originalCursorLock;
        Cursor.visible = originalCursorVisible;

        isInteracting = false;
        isTransitioning = false;
    }
}
