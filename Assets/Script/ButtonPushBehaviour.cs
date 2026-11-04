using UnityEngine;

public class ButtonPushBehaviour : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 3f;

    private void Update()
    {
        if (playerCamera == null) return;

        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // POWER BUTTON
            ButtonBehaviour powerButton =
                hit.collider.GetComponentInParent<ButtonBehaviour>();

            if (powerButton != null)
            {
                powerButton.PressButton();
                return;
            }

            // TEMPERATURE BUTTON
            TempControlBehaviour tempButton =
    hit.collider.GetComponentInParent<TempControlBehaviour>();

            if (tempButton != null)
            {
                tempButton.RepairSystem();
                return;
            }
        }
    }
}
