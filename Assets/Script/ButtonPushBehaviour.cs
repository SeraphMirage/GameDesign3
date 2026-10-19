using UnityEngine;

public class ButtonPushBehaviour : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 3f;

    private void Update()
    {
        if (playerCamera == null) return;

        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            ButtonBehaviour button =
                hit.collider.GetComponentInParent<ButtonBehaviour>();

            if (button != null)
            {
                button.PressButton();
            }
        }
    }
}
