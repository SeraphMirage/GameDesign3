using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterBehaviour : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 1f;

    [Header("Vision")]
    public float sightDistance = 12f;

    [Range(0f, 360f)]
    public float fieldOfView = 90f;

    public float eyeHeight = 1f;

    private NavMeshAgent agent;

    private int currentPatrolPoint = 0;
    private float waitTimer = 0f;

    private bool canSeePlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Automatically find the Player.
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError(
                "Monster cannot find the Player. Make sure the Player is tagged 'Player'.",
                this
            );
        }

        // Start the patrol if the monster is correctly on the NavMesh.
        if (agent.isOnNavMesh)
        {
            GoToCurrentPatrolPoint();
        }
        else
        {
            Debug.LogWarning(
                "Monster is not standing on the NavMesh.",
                this
            );
        }
    }

    void Update()
    {
        if (player == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        // Check whether the monster can currently see the player.
        CheckLineOfSight();

        if (canSeePlayer)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        agent.speed = patrolSpeed;

        if (agent.pathPending)
            return;

        // Monster reached its patrol point.
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= patrolWaitTime)
            {
                waitTimer = 0f;

                currentPatrolPoint++;

                // Loop back to the first patrol point.
                if (currentPatrolPoint >= patrolPoints.Length)
                {
                    currentPatrolPoint = 0;
                }

                GoToCurrentPatrolPoint();
            }
        }
    }

    void GoToCurrentPatrolPoint()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (patrolPoints[currentPatrolPoint] == null)
            return;

        agent.speed = patrolSpeed;

        agent.SetDestination(
            patrolPoints[currentPatrolPoint].position
        );
    }

    void ChasePlayer()
    {
        if (player == null)
            return;

        agent.speed = chaseSpeed;

        agent.SetDestination(
            player.position
        );
    }

    void CheckLineOfSight()
    {
        canSeePlayer = false;

        // Monster's eye position.
        Vector3 eyePosition =
            transform.position + Vector3.up * eyeHeight;

        // Aim approximately toward the center of the Player.
        Vector3 playerTarget =
            player.position + Vector3.up * 0.5f;

        Vector3 directionToPlayer =
            playerTarget - eyePosition;

        float distanceToPlayer =
            directionToPlayer.magnitude;

        // Player is outside sight distance.
        if (distanceToPlayer > sightDistance)
            return;

        // Ignore vertical difference for the field-of-view angle.
        Vector3 flatDirection =
            new Vector3(
                directionToPlayer.x,
                0f,
                directionToPlayer.z
            );

        if (flatDirection.sqrMagnitude <= 0.001f)
            return;

        flatDirection.Normalize();

        float angleToPlayer =
            Vector3.Angle(
                transform.forward,
                flatDirection
            );

        // Player is behind the monster or outside its vision cone.
        if (angleToPlayer > fieldOfView / 2f)
            return;

        // Check if a wall/object is blocking the Player.
        if (Physics.Raycast(
            eyePosition,
            directionToPlayer.normalized,
            out RaycastHit hit,
            distanceToPlayer
        ))
        {
            // Works if the collider is directly on the Player
            // or on one of the Player's child objects.
            if (
                hit.transform.CompareTag("Player") ||
                hit.transform.root.CompareTag("Player")
            )
            {
                canSeePlayer = true;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 eyePosition =
            transform.position + Vector3.up * eyeHeight;

        // Sight distance.
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            eyePosition,
            sightDistance
        );

        // Left side of field of view.
        Vector3 leftDirection =
            Quaternion.Euler(
                0f,
                -fieldOfView / 2f,
                0f
            ) * transform.forward;

        // Right side of field of view.
        Vector3 rightDirection =
            Quaternion.Euler(
                0f,
                fieldOfView / 2f,
                0f
            ) * transform.forward;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            eyePosition,
            leftDirection * sightDistance
        );

        Gizmos.DrawRay(
            eyePosition,
            rightDirection * sightDistance
        );

        // Blue line shows exactly which direction
        // Unity considers the monster's "front".
        Gizmos.color = Color.blue;

        Gizmos.DrawRay(
            eyePosition,
            transform.forward * sightDistance
        );
    }
}