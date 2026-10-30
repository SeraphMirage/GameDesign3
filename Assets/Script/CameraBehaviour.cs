using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform Player;

    public Vector3 firstPersonOffset = new Vector3(0f, 1.6f, 0f);
    public float rotateSpeed = 5f;
    public float minVerticalAngle = -35f;
    public float maxVerticalAngle = 60f;

    private float mouseX;
    private float mouseY;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (Player != null)
        {
            mouseX = Player.eulerAngles.y;
        }
    }

    void LateUpdate()
    {
        if (Player == null) return;

        mouseX += Input.GetAxis("Mouse X") * rotateSpeed;
        mouseY -= Input.GetAxis("Mouse Y") * rotateSpeed;

        mouseY = Mathf.Clamp(
            mouseY,
            minVerticalAngle,
            maxVerticalAngle
        );

        Player.rotation = Quaternion.Euler(0f, mouseX, 0f);

        transform.position = Player.position + firstPersonOffset;
        transform.rotation = Quaternion.Euler(mouseY, mouseX, 0f);
    }
}
