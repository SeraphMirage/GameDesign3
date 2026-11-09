
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
    public float patrolWaitTime = 1f;

    [Header("Top Deck")]
    public Transform topDeckSpawn;
    public Transform[] topDeckPatrolPoints;
    public BoxCollider topDeckArea;

    [Header("Bottom Deck")]
    public Transform bottomDeckSpawn;
    public Transform[] bottomDeckPatrolPoints;
    public BoxCollider bottomDeckArea;

    [Header("Vision")]
    public float sightDistance = 12f;

    [Range(0f, 360f)]
    public float fieldOfView = 90f;

    public float eyeHeight = 1f;

    private NavMeshAgent agent;

    private Transform[] patrolPoints;
    private int currentPatrolPoint = 0;
    private float waitTimer = 0f;

    private bool canSeePlayer = false;
    private string currentDeck = "";

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Find Player automatically.
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
            else
                Debug.LogError(
                    "Monster cannot find Player!",
                    this
                );
        }

        // Synchronize with DeckStatusManager.
        SyncWithDeckManager();
    }

    void Update()
    {
        // Keep monster assigned to the correct deck.
        SyncWithDeckManager();

        if (player == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (currentDeck == "")
            return;

        CheckLineOfSight();

        if (canSeePlayer)
            ChasePlayer();
        else
            Patrol();
    }

    void SyncWithDeckManager()
    {
        if (DeckStatusManager.Instance == null)
            return;

        string assignedDeck =
            DeckStatusManager.Instance.monsterDeck;

        // Only relocate when the assigned deck changes.
        if (assignedDeck != currentDeck)
        {
            ChangeDeck(assignedDeck);
        }
    }

    void ChangeDeck(string newDeck)
    {
        Transform destination;

        if (newDeck == "Top Deck")
        {
            destination = topDeckSpawn;
            patrolPoints = topDeckPatrolPoints;
        }
        else if (newDeck == "Bottom Deck")
        {
            destination = bottomDeckSpawn;
            patrolPoints = bottomDeckPatrolPoints;
        }
        else
        {
            Debug.LogWarning("Unknown deck: " + newDeck);
            return;
        }

        if (destination == null)
        {
            Debug.LogWarning(
                "Monster spawn is missing for " + newDeck
            );
            return;
        }

        // Find the NavMesh near the deck spawn.
        NavMeshHit hit;

        if (!NavMesh.SamplePosition(
            destination.position,
            out hit,
            1.5f,
            NavMesh.AllAreas))
        {
            Debug.LogWarning(
                "No NavMesh near spawn for " + newDeck
            );
            return;
        }

        // Warp is safer than setting transform.position
        // directly when using a NavMeshAgent.
        agent.ResetPath();

        if (!agent.Warp(hit.position))
        {
            Debug.LogWarning(
                "Monster could not warp to " + newDeck
            );
            return;
        }

        currentDeck = newDeck;

        currentPatrolPoint = 0;
        waitTimer = 0f;
        canSeePlayer = false;

        Debug.Log(
            "Monster physically moved to: " + currentDeck
        );

        GoToCurrentPatrolPoint();
    }

    bool IsPlayerOnCurrentDeck()
    {
        BoxCollider area = null;

        if (currentDeck == "Top Deck")
            area = topDeckArea;
        else if (currentDeck == "Bottom Deck")
            area = bottomDeckArea;

        if (area == null || player == null)
            return false;

        return area.bounds.Contains(player.position);
    }

    void Patrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        agent.speed = patrolSpeed;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <=
            agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= patrolWaitTime)
            {
                waitTimer = 0f;

                currentPatrolPoint++;

                if (currentPatrolPoint >=
                    patrolPoints.Length)
                {
                    currentPatrolPoint = 0;
                }

                GoToCurrentPatrolPoint();
            }
        }
    }

    void GoToCurrentPatrolPoint()
    {
        if (!agent.isOnNavMesh)
            return;

        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        Transform point =
            patrolPoints[currentPatrolPoint];

        if (point == null)
            return;

        agent.speed = patrolSpeed;
        agent.SetDestination(point.position);
    }

    void ChasePlayer()
    {
        if (player == null)
            return;

        // Never chase someone on a different deck.
        if (!IsPlayerOnCurrentDeck())
        {
            canSeePlayer = false;
            GoToCurrentPatrolPoint();
            return;
        }

        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void CheckLineOfSight()
    {
        canSeePlayer = false;

        // Only detect a player on the assigned deck.
        if (!IsPlayerOnCurrentDeck())
            return;

        Vector3 eyePosition =
            transform.position + Vector3.up * eyeHeight;

        Vector3 playerTarget =
            player.position + Vector3.up * 0.5f;

        Vector3 directionToPlayer =
            playerTarget - eyePosition;

        float distanceToPlayer =
            directionToPlayer.magnitude;

        if (distanceToPlayer > sightDistance)
            return;

        Vector3 flatDirection = new Vector3(
            directionToPlayer.x,
            0f,
            directionToPlayer.z
        );

        if (flatDirection.sqrMagnitude <= 0.001f)
            return;

        flatDirection.Normalize();

        float angleToPlayer = Vector3.Angle(
            transform.forward,
            flatDirection
        );

        if (angleToPlayer > fieldOfView / 2f)
            return;

        if (Physics.Raycast(
            eyePosition,
            directionToPlayer.normalized,
            out RaycastHit hit,
            distanceToPlayer))
        {
            if (hit.transform.CompareTag("Player") ||
                hit.transform.root.CompareTag("Player"))
            {
                canSeePlayer = true;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 eyePosition =
            transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            eyePosition,
            sightDistance
        );

        Vector3 leftDirection =
            Quaternion.Euler(
                0f, -fieldOfView / 2f, 0f
            ) * transform.forward;

        Vector3 rightDirection =
            Quaternion.Euler(
                0f, fieldOfView / 2f, 0f
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

        Gizmos.color = Color.blue;

        Gizmos.DrawRay(
            eyePosition,
            transform.forward * sightDistance
        );
    }
}
