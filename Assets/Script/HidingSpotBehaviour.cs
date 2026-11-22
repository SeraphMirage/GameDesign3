
using UnityEngine;

public class HidingSpotBehaviour : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Interaction")]
    public KeyCode interactionKey = KeyCode.E;
    public float interactionDistance = 2f;

    [Header("Hiding Positions")]
    public Transform hidePoint;
    public Transform exitPoint;

    [Header("Player Movement")]
    [Tooltip("Assign movement scripts, not camera scripts.")]
    public Behaviour[] playerMovementScripts;

    [Header("Hiding Settings")]
    public bool monsterCanSeeInside = false;
    public bool hidePlayerModel = true;

    [Header("Camera")]
    public bool allowCameraLook = true;

    private CharacterController controller;
    private Rigidbody playerRigidbody;

    private Renderer[] playerRenderers;
    private bool[] originalRendererStates;
    private bool[] originalMovementStates;

    private bool originalKinematic;
    private bool originalGravity;
    private RigidbodyConstraints originalConstraints;

    private static HidingSpotBehaviour currentSpot;

    private bool isPlayerHiding = false;

    // ------------------------------------
    // START
    // ------------------------------------

    void Start()
    {
        FindPlayer();
    }

    // ------------------------------------
    // UPDATE
    // ------------------------------------

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (isPlayerHiding)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                ExitHidingSpot();
            }

            return;
        }

        // Prevent using multiple hiding spots.
        if (currentSpot != null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance > interactionDistance)
            return;

        if (Input.GetKeyDown(interactionKey))
        {
            EnterHidingSpot();
        }
    }

    // ------------------------------------
    // FIND PLAYER
    // ------------------------------------

    void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
            return;

        player = playerObject.transform;

        controller =
            player.GetComponent<CharacterController>();

        playerRigidbody =
            player.GetComponent<Rigidbody>();

        playerRenderers =
            player.GetComponentsInChildren<Renderer>(true);
    }

    // ------------------------------------
    // ENTER HIDING SPOT
    // ------------------------------------

    public void EnterHidingSpot()
    {
        if (player == null || hidePoint == null)
            return;

        if (currentSpot != null)
            return;

        currentSpot = this;
        isPlayerHiding = true;

        // Stop the player's existing movement.
        PlayerMovementBehaviour playerMovement =
            player.GetComponent<PlayerMovementBehaviour>();

        if (playerMovement != null)
        {
            playerMovement.StopMovement();
        }

        // Save and disable walking scripts.
        originalMovementStates =
            new bool[playerMovementScripts.Length];

        for (int i = 0;
             i < playerMovementScripts.Length;
             i++)
        {
            Behaviour movementScript =
                playerMovementScripts[i];

            if (movementScript == null)
                continue;

            originalMovementStates[i] =
                movementScript.enabled;

            movementScript.enabled = false;
        }

        // Save renderer settings.
        originalRendererStates =
            new bool[playerRenderers.Length];

        for (int i = 0;
             i < playerRenderers.Length;
             i++)
        {
            originalRendererStates[i] =
                playerRenderers[i].enabled;
        }

        // Freeze Rigidbody while hiding.
        if (playerRigidbody != null)
        {
            originalKinematic =
                playerRigidbody.isKinematic;

            originalGravity =
                playerRigidbody.useGravity;

            originalConstraints =
                playerRigidbody.constraints;

            playerRigidbody.linearVelocity =
                Vector3.zero;

            playerRigidbody.angularVelocity =
                Vector3.zero;

            playerRigidbody.isKinematic = true;
            playerRigidbody.useGravity = false;
        }

        // Disable CharacterController for teleport.
        if (controller != null)
            controller.enabled = false;

        // Move into hiding spot.
        player.SetPositionAndRotation(
            hidePoint.position,
            hidePoint.rotation
        );

        if (controller != null)
            controller.enabled = true;

        // Hide player model.
        if (hidePlayerModel)
        {
            foreach (Renderer rend in playerRenderers)
            {
                if (rend != null)
                    rend.enabled = false;
            }
        }

        Debug.Log(
            "Player entered hiding spot: " +
            gameObject.name
        );
    }

    // ------------------------------------
    // EXIT HIDING SPOT
    // ------------------------------------

    public void ExitHidingSpot()
    {
        if (!isPlayerHiding)
            return;

        // Move player outside hiding spot.
        if (exitPoint != null)
        {
            if (controller != null)
                controller.enabled = false;

            player.SetPositionAndRotation(
                exitPoint.position,
                exitPoint.rotation
            );

            if (controller != null)
                controller.enabled = true;
        }

        // Restore Rigidbody physics.
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic =
                originalKinematic;

            playerRigidbody.useGravity =
                originalGravity;

            playerRigidbody.constraints =
                originalConstraints;
        }

        // Restore character visibility.
        if (originalRendererStates != null)
        {
            for (int i = 0;
                 i < playerRenderers.Length;
                 i++)
            {
                if (playerRenderers[i] != null)
                {
                    playerRenderers[i].enabled =
                        originalRendererStates[i];
                }
            }
        }

        // Restore walking scripts.
        if (originalMovementStates != null)
        {
            for (int i = 0;
                 i < playerMovementScripts.Length;
                 i++)
            {
                if (playerMovementScripts[i] != null)
                {
                    playerMovementScripts[i].enabled =
                        originalMovementStates[i];
                }
            }
        }

        isPlayerHiding = false;
        currentSpot = null;

        Debug.Log(
            "Player exited hiding spot: " +
            gameObject.name
        );
    }

    // ------------------------------------
    // MONSTER DETECTION
    // ------------------------------------

    public static bool IsPlayerHidden(
        Transform targetPlayer)
    {
        if (currentSpot == null ||
            targetPlayer == null ||
            currentSpot.player == null)
            return false;

        return currentSpot.isPlayerHiding &&
            targetPlayer.root ==
            currentSpot.player.root;
    }

    public static bool CanMonsterSeePlayer(
        Transform targetPlayer)
    {
        if (!IsPlayerHidden(targetPlayer))
            return true;

        return currentSpot.monsterCanSeeInside;
    }

    // ------------------------------------
    // GIZMOS
    // ------------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionDistance
        );

        if (hidePoint != null)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(
                hidePoint.position,
                0.25f
            );
        }

        if (exitPoint != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                exitPoint.position,
                0.25f
            );
        }
    }

    // ------------------------------------
    // CLEANUP
    // ------------------------------------

    void OnDisable()
    {
        if (isPlayerHiding)
        {
            ExitHidingSpot();
        }
    }
}
