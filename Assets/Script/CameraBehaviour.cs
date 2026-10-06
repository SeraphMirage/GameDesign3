using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform Player;         
    public Vector3 offset = new Vector3(0f, 2f, -4f); 
    public float rotateSpeed = 5f;   
    public float minVerticalAngle = -35f;
    public float maxVerticalAngle = 60f;

    private float mouseX, mouseY;


    void LateUpdate()
    {
        if (!Player) return;

        // Get mouse input
        mouseX += Input.GetAxis("Mouse X") * rotateSpeed;
        mouseY -= Input.GetAxis("Mouse Y") * rotateSpeed;
        mouseY = Mathf.Clamp(mouseY, minVerticalAngle, maxVerticalAngle);

        // Rotate camera around the player
        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0);
        Vector3 position = Player.position + rotation * offset;

        transform.rotation = rotation;
        transform.position = position;
    }
}
