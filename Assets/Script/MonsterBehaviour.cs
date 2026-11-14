
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[System.Serializable]
public class MonsterSpawnLocation
{
    [Header("Spawn Position")]
    public Transform spawnPoint;

    [Header("Patrol Route")]
    public Transform[] patrolPoints;
}

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

    [Header("Scene Assignment")]
    [Tooltip("Must match Top Deck or Bottom Deck")]
    public string thisDeck = "Top Deck";

    [Header("Random Spawn Locations")]
    public MonsterSpawnLocation[] spawnLocations;

    [Header("Vision")]
    public float sightDistance = 12f;

    [Range(0f, 360f)]
    public float fieldOfView = 90f;

    public float eyeHeight = 1f;

    [Header("Spawn Settings")]
    public float navMeshSearchRadius = 2f;

    [Header("Search Behaviour")]
    public float searchSpeed = 3f;
    public float searchDuration = 8f;
    public float searchRadius = 5f;

    private NavMeshAgent agent;

    private Transform[] patrolPoints;
    private int currentPatrolPoint;
    private float waitTimer;

    private bool canSeePlayer;
    private bool monsterActive;
    private bool lastDeckMatch;

    // ------------------------------------
    // MONSTER STATES
    // ------------------------------------

    private enum MonsterState
    {
        Patrol,
        Chase,
        Search
    }

    private MonsterState currentState =
        MonsterState.Patrol;

    private Vector3 lastKnownPlayerPosition;
    private float searchTimer;
    private bool searchingLastKnownPosition;

    // ------------------------------------
    // START AND UPDATE
    // ------------------------------------

    void Awake()
{
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        FindPlayer();

        // Hide monster until the deck manager
        // confirms which deck it belongs to.
        SetMonsterActive(false);

        SyncWithDeckManager();
    }

    void Update()
{
        SyncWithDeckManager();

        if (!monsterActive)
        return;

        if (player == null)
        {
            FindPlayer();
        }

        if (!agent.isOnNavMesh)
            return;

        // Patrol even when the player is missing.
        if (player != null)
    {
            CheckLineOfSight();

            if (canSeePlayer)
    {
                lastKnownPlayerPosition =
                    player.position;

                if (currentState != MonsterState.Chase)
                {
                    currentState = MonsterState.Chase;

                    agent.ResetPath();
}
    }
            else if (currentState == MonsterState.Chase)
            {
                StartSearching();
            }
        }
        else
        {
            canSeePlayer = false;

            if (currentState == MonsterState.Chase)
            {
                currentState = MonsterState.Patrol;
                GoToCurrentPatrolPoint();
            }
        }

        // Perform the current monster behaviour.
        switch (currentState)
        {
            case MonsterState.Patrol:
                Patrol();
                break;

            case MonsterState.Chase:
                ChasePlayer();
                break;

            case MonsterState.Search:
                SearchForPlayer();
                break;
        }
    }

    // ------------------------------------
    // PLAYER
    // ------------------------------------

    void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    // ------------------------------------
    // DECK SYSTEM
    // ------------------------------------

    void SyncWithDeckManager()
    {
        if (DeckStatusManager.Instance == null)
            return;

        bool deckMatches =
            DeckStatusManager.Instance
            .IsMonsterOnDeck(thisDeck);

        if (deckMatches && !lastDeckMatch)
        {
            SpawnMonster();
        }
        else if (!deckMatches && lastDeckMatch)
        {
            SetMonsterActive(false);
        }

        // Only mark the deck as matched when
        // the monster actually spawned.
        lastDeckMatch = deckMatches && monsterActive;
    }

    // ------------------------------------
    // RANDOM SPAWN SYSTEM
    // ------------------------------------

    public void SpawnMonster()
    {
        if (DeckStatusManager.Instance == null)
            return;

        if (!DeckStatusManager.Instance
            .IsMonsterOnDeck(thisDeck))
            return;

        if (spawnLocations == null ||
            spawnLocations.Length == 0)
        {
            Debug.LogWarning(
                "No spawn locations assigned on " +
                thisDeck, this
            );
            return;
        }

        if (agent == null)
            return;

        // Collect valid spawn locations.
        List<int> validIndices =
            new List<int>();

        List<Vector3> validPositions =
            new List<Vector3>();

        for (int i = 0;
             i < spawnLocations.Length;
             i++)
        {
            MonsterSpawnLocation location =
                spawnLocations[i];

            if (location == null ||
                location.spawnPoint == null)
                continue;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                location.spawnPoint.position,
                out hit,
                navMeshSearchRadius,
                agent.areaMask))
            {
                validIndices.Add(i);
                validPositions.Add(hit.position);
            }
        }

        if (validIndices.Count == 0)
        {
            Debug.LogWarning(
                "No valid NavMesh spawn points on " +
                thisDeck, this
            );
            return;
        }

        // Choose a random spawn location.
        int choice =
            DeckStatusManager.Instance
            .GetRandomSpawnIndex(
                thisDeck,
                validIndices.Count
            );

        if (choice < 0 ||
            choice >= validIndices.Count)
            return;

        int selectedIndex =
            validIndices[choice];

        MonsterSpawnLocation selected =
            spawnLocations[selectedIndex];

        Vector3 spawnPosition =
            validPositions[choice];

        // Warp the monster onto the NavMesh.
        agent.enabled = true;

        if (!agent.Warp(spawnPosition))
        {
            Debug.LogWarning(
                "Monster could not warp to spawn!",
                this
            );
            return;
        }

        agent.ResetPath();

        // Assign the selected patrol route.
        patrolPoints = selected.patrolPoints;

        currentPatrolPoint = 0;
        waitTimer = 0f;

        canSeePlayer = false;

        currentState = MonsterState.Patrol;

        searchTimer = 0f;
        searchingLastKnownPosition = false;

        SetMonsterActive(true);

        GoToCurrentPatrolPoint();

        Debug.Log(
            "Monster spawned on " +
            thisDeck +
            " at Spawn Point " +
            (selectedIndex + 1)
        );
    }

    // Manually respawn the monster on
    // the currently assigned deck.
    public void RespawnMonster()
    {
        SetMonsterActive(false);
        SpawnMonster();
    }

    // ------------------------------------
    // VISIBILITY AND MOVEMENT
    // ------------------------------------

    void SetMonsterActive(bool active)
    {
        monsterActive = active;

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            if (active)
            {
                agent.isStopped = false;
            }
            else
            {
                agent.ResetPath();
                agent.isStopped = true;
            }
        }

        // Hide or show monster renderers.
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in renderers)
        {
            rend.enabled = active;
        }

        // Disable physical monster colliders
        // while it is on the other deck.
        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            col.enabled = active;
        }
    }

    // ------------------------------------
    // PATROL SYSTEM
    // ------------------------------------

    void Patrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        agent.speed = patrolSpeed;

        if (agent.pathPending)
            return;

        if (!agent.hasPath)
        {
            GoToCurrentPatrolPoint();
            return;
        }

        if (agent.remainingDistance <=
            agent.stoppingDistance + 0.1f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= patrolWaitTime)
            {
                waitTimer = 0f;

                currentPatrolPoint =
                    (currentPatrolPoint + 1) %
                    patrolPoints.Length;

                GoToCurrentPatrolPoint();
            }
        }
        else
        {
            waitTimer = 0f;
        }
    }

    void GoToCurrentPatrolPoint()
    {
        if (!agent.isOnNavMesh)
            return;

        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        // Skip empty patrol points.
        for (int count = 0;
             count < patrolPoints.Length;
             count++)
        {
            Transform point =
                patrolPoints[currentPatrolPoint];

            if (point != null)
            {
                agent.speed = patrolSpeed;
                agent.isStopped = false;

                agent.SetDestination(
                    point.position
                );

                return;
            }

            currentPatrolPoint =
                (currentPatrolPoint + 1) %
                patrolPoints.Length;
        }

        Debug.LogWarning(
            "All patrol points are empty for " +
            thisDeck, this
        );
    }

    // ------------------------------------
    // CHASE SYSTEM
    // ------------------------------------

    void ChasePlayer()
    {
        if (player == null ||
            !agent.isOnNavMesh)
            return;

        agent.speed = chaseSpeed;
        agent.isStopped = false;

        agent.SetDestination(
            player.position
        );
    }

    // ------------------------------------
    // SEARCH SYSTEM
    // ------------------------------------

    void StartSearching()
    {
        if (!agent.isOnNavMesh)
            return;

        currentState = MonsterState.Search;

        searchTimer = 0f;

        searchingLastKnownPosition = true;

        agent.ResetPath();

        agent.speed = searchSpeed;
        agent.isStopped = false;

        agent.SetDestination(
            lastKnownPlayerPosition
        );
    }

    void SearchForPlayer()
    {
        if (!agent.isOnNavMesh)
            return;

        searchTimer += Time.deltaTime;

        // Stop searching after the timer ends.
        if (searchTimer >= searchDuration)
        {
            currentState = MonsterState.Patrol;

            waitTimer = 0f;

            GoToCurrentPatrolPoint();

            return;
        }

        agent.speed = searchSpeed;
        agent.isStopped = false;

        if (agent.pathPending)
            return;

        // Search around the player's
        // last known location.
        if (!agent.hasPath ||
            agent.remainingDistance <=
            agent.stoppingDistance + 0.3f ||
            agent.pathStatus ==
            NavMeshPathStatus.PathInvalid)
        {
            if (searchingLastKnownPosition)
            {
                searchingLastKnownPosition = false;
            }

            Vector3 randomOffset =
                Random.insideUnitSphere *
                Mathf.Max(0f, searchRadius);

            randomOffset.y = 0f;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                lastKnownPlayerPosition + randomOffset,
                out hit,
                navMeshSearchRadius,
                agent.areaMask))
            {
                agent.SetDestination(
                    hit.position
                );
            }
        }
    }

    // ------------------------------------
    // VISION SYSTEM
    // ------------------------------------

    void CheckLineOfSight()
    {
        canSeePlayer = false;

        if (player == null)
            return;

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

        if (distanceToPlayer > sightDistance ||
            distanceToPlayer < 0.001f)
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

    // ------------------------------------
    // GIZMOS
    // ------------------------------------

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

        Gizmos.color = Color.blue;

        Gizmos.DrawRay(
            eyePosition,
            transform.forward * sightDistance
        );
    }
}
