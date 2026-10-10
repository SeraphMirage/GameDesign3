using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform Player;
    public Vector3 offset = new Vector3(0f, 2f, -4f);
    public float rotateSpeed = 5f;
    public float minVerticalAngle = -35f;
    public float maxVerticalAngle = 60f;

    [Header("Camera Collision")]
    public float collisionRadius = 0.2f;
    public float collisionPadding = 0.1f;
    public float cameraSmoothSpeed = 15f;

    private float mouseX, mouseY;
    private float currentDistance;

    void Start()
    {
        currentDistance = offset.magnitude;
    }

    void LateUpdate()
    {
        if (!Player) return;

        mouseX += Input.GetAxis("Mouse X") * rotateSpeed;
        mouseY -= Input.GetAxis("Mouse Y") * rotateSpeed;

        mouseY = Mathf.Clamp(
            mouseY,
            minVerticalAngle,
            maxVerticalAngle
        );

        // Camera orbit rotation
        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0f);

        // Desired camera position
        Vector3 desiredPosition = Player.position + rotation * offset;

        // Direction from player to camera
        Vector3 direction = desiredPosition - Player.position;
        float distance = direction.magnitude;
        direction.Normalize();

        // Check for walls
        float targetDistance = distance;

        if (Physics.SphereCast(
            Player.position,
            collisionRadius,
            direction,
            out RaycastHit hit,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            targetDistance = Mathf.Max(
                0.05f,
                hit.distance - collisionPadding
            );
        }

        // Move in quickly, move out smoothly
        if (targetDistance < currentDistance)
        {
            currentDistance = targetDistance;
        }
        else
        {
            currentDistance = Mathf.Lerp(
                currentDistance,
                targetDistance,
                cameraSmoothSpeed * Time.deltaTime
            );
        }

        // Apply final position and rotation
        transform.position = Player.position + direction * currentDistance;
        transform.rotation = rotation;
    }
}
