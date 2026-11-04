using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterBehaviour : MonoBehaviour
{
    [Header("Deck")]
    public string deckName = "Bottom Deck";

    [Header("Spawn")]
    public Transform spawnPoint;

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

    private bool canSeePlayer;
    private bool monsterPresent;
    private bool initialized;

    private Renderer[] monsterRenderers;
    private Collider[] monsterColliders;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        monsterRenderers =
            GetComponentsInChildren<Renderer>(true);

        monsterColliders =
            GetComponentsInChildren<Collider>(true);

        FindPlayer();

        // Start hidden until we know which deck owns the monster.
        SetVisible(false);

        CheckDeckPresence();
    }

    void Update()
    {
        if (DeckStatusManager.Instance == null)
            return;

        bool shouldBeHere =
            DeckStatusManager.Instance.IsMonsterOnDeck(deckName);

        // Monster changed decks.
        if (!initialized || shouldBeHere != monsterPresent)
        {
            CheckDeckPresence();
        }

        if (!monsterPresent)
            return;

        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        if (player == null)
            FindPlayer();

        if (player == null)
            return;

        CheckLineOfSight();

        if (canSeePlayer)
            ChasePlayer();
        else
            Patrol();
    }

    void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void CheckDeckPresence()
    {
        if (DeckStatusManager.Instance == null)
            return;

        monsterPresent =
            DeckStatusManager.Instance.IsMonsterOnDeck(deckName);

        initialized = true;

        if (monsterPresent)
            SpawnMonster();
        else
            HideMonster();
    }

    void SpawnMonster()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "No Monster Spawn Point assigned on " + deckName
            );

            return;
        }

        // Stop the agent while repositioning.
        if (agent.enabled)
        {
            if (agent.isOnNavMesh)
                agent.ResetPath();

            agent.enabled = false;
        }

        // Find the NavMesh near this deck's spawn point.
        if (NavMesh.SamplePosition(
            spawnPoint.position,
            out NavMeshHit hit,
            5f,
            NavMesh.AllAreas))
        {
            transform.position = hit.position;
            transform.rotation = spawnPoint.rotation;
        }
        else
        {
            Debug.LogError(
                "Monster spawn point is not near a NavMesh on " +
                deckName
            );

            return;
        }

        // Make monster visible.
        SetVisible(true);

        // Turn NavMesh movement back on.
        agent.enabled = true;

        agent.speed = patrolSpeed;

        currentPatrolPoint = 0;
        waitTimer = 0f;
        canSeePlayer = false;

        if (agent.isOnNavMesh)
        {
            GoToCurrentPatrolPoint();

            Debug.Log(
                "MONSTER APPEARED ON: " + deckName
            );
        }
        else
        {
            Debug.LogError(
                "Monster spawned but is not on NavMesh: " +
                deckName
            );
        }
    }

    void HideMonster()
    {
        canSeePlayer = false;

        if (agent.enabled)
        {
            if (agent.isOnNavMesh)
                agent.ResetPath();

            agent.enabled = false;
        }

        SetVisible(false);

        Debug.Log(
            "MONSTER DISAPPEARED FROM: " + deckName
        );
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer renderer in monsterRenderers)
        {
            renderer.enabled = visible;
        }

        foreach (Collider collider in monsterColliders)
        {
            collider.enabled = visible;
        }
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
            agent.stoppingDistance + 0.2f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= patrolWaitTime)
            {
                waitTimer = 0f;

                currentPatrolPoint++;

                if (currentPatrolPoint >= patrolPoints.Length)
                    currentPatrolPoint = 0;

                GoToCurrentPatrolPoint();
            }
        }
    }

    void GoToCurrentPatrolPoint()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        Transform point =
            patrolPoints[currentPatrolPoint];

        if (point == null)
            return;

        if (NavMesh.SamplePosition(
            point.position,
            out NavMeshHit hit,
            3f,
            agent.areaMask))
        {
            agent.SetDestination(hit.position);
        }
    }

    void ChasePlayer()
    {
        if (player == null)
            return;

        agent.speed = chaseSpeed;

        if (NavMesh.SamplePosition(
            player.position,
            out NavMeshHit hit,
            3f,
            agent.areaMask))
        {
            agent.SetDestination(hit.position);
        }
    }

    void CheckLineOfSight()
    {
        canSeePlayer = false;

        Vector3 eyePosition =
            transform.position +
            Vector3.up * eyeHeight;

        Vector3 playerTarget =
            player.position +
            Vector3.up * 0.5f;

        Vector3 directionToPlayer =
            playerTarget - eyePosition;

        float distanceToPlayer =
            directionToPlayer.magnitude;

        if (distanceToPlayer > sightDistance)
            return;

        Vector3 flatDirection =
            new Vector3(
                directionToPlayer.x,
                0f,
                directionToPlayer.z
            );

        if (flatDirection.sqrMagnitude <= 0.001f)
            return;

        flatDirection.Normalize();

        float angle =
            Vector3.Angle(
                transform.forward,
                flatDirection
            );

        if (angle > fieldOfView / 2f)
            return;

        if (Physics.Raycast(
            eyePosition,
            directionToPlayer.normalized,
            out RaycastHit hit,
            distanceToPlayer))
        {
            if (
                hit.transform.CompareTag("Player") ||
                hit.transform.root.CompareTag("Player"))
            {
                canSeePlayer = true;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 eyePosition =
            transform.position +
            Vector3.up * eyeHeight;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            eyePosition,
            sightDistance
        );

        Vector3 leftDirection =
            Quaternion.Euler(
                0f,
                -fieldOfView / 2f,
                0f
            ) * transform.forward;

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
    }
}