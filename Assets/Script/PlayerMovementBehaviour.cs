using UnityEngine;

public class PlayerMovementBehaviour : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private Vector3 moveInput;

    public Transform cameraTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        moveInput = new Vector3(moveX, 0f, moveZ).normalized;
    }

    void FixedUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection =
            (forward * moveInput.z + right * moveInput.x).normalized;

        Vector3 velocity = rb.linearVelocity;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            velocity.x = moveDirection.x * moveSpeed;
            velocity.z = moveDirection.z * moveSpeed;

            rb.linearVelocity = velocity;

            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            Quaternion newRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(newRotation);
        }
        else
        {
            // Instantly stop horizontal movement
            velocity.x = 0f;
            velocity.z = 0f;

            rb.linearVelocity = velocity;
        }
    }
}
