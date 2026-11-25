
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
    [Tooltip("Top Deck or Bottom Deck")]
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

    [Header("Deck Decision")]
    [Range(0f, 1f)]
    public float leaveDeckAfterSearchChance = 0.5f;

    private NavMeshAgent agent;

    private Transform[] patrolPoints;
    private int currentPatrolPoint;
    private float waitTimer;

    private bool canSeePlayer;
    private bool monsterActive;

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
    // AWAKE
    // ------------------------------------

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // ------------------------------------
    // START
    // ------------------------------------

    void Start()
    {
        FindPlayer();

        SetMonsterActive(false);

        if (DeckStatusManager.Instance != null)
        {
            DeckStatusManager.Instance.RegisterMonster(
                thisDeck,
                this
            );
        }

        SyncWithDeckManager();
    }

    // ------------------------------------
    // UPDATE
    // ------------------------------------

    void Update()
    {
        SyncWithDeckManager();

        if (!monsterActive)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (player == null)
            FindPlayer();

        if (player != null)
        {
            CheckLineOfSight();

            if (canSeePlayer)
            {
                lastKnownPlayerPosition =
                    player.position;

                if (currentState != MonsterState.Chase)
                {
                    currentState =
                        MonsterState.Chase;

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
                currentState =
                    MonsterState.Patrol;

                GoToCurrentPatrolPoint();
            }
        }

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
    // FIND PLAYER
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

        if (deckMatches && !monsterActive)
        {
            SpawnMonster();
        }
        else if (!deckMatches && monsterActive)
        {
            SetMonsterActive(false);
        }
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
            return;

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
                "No valid NavMesh spawn locations " +
                "on " + thisDeck,
                this
            );
            return;
        }

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

        agent.enabled = true;

        if (!agent.Warp(spawnPosition))
        {
            Debug.LogWarning(
                "Monster failed to warp.",
                this
            );
            return;
        }

        agent.ResetPath();

        patrolPoints =
            selected.patrolPoints;

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

    public void RespawnMonster()
    {
        SetMonsterActive(false);
        SpawnMonster();
    }

    // ------------------------------------
    // VISIBILITY
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

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in renderers)
        {
            rend.enabled = active;
        }

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

        if (!agent.hasPath ||
            agent.remainingDistance <=
            agent.stoppingDistance + 0.2f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= patrolWaitTime)
            {
                waitTimer = 0f;

                // Reached the final patrol point.
                bool finishedPatrol =
                    currentPatrolPoint ==
                    patrolPoints.Length - 1;

                // Only switch decks when a
                // patrol route has finished.
                if (finishedPatrol &&
                    IsDeckChangePending())
                {
                    ChangeDeckAfterPatrol();
                    return;
                }

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
            "All patrol points are empty on " +
            thisDeck,
            this
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

        if (searchTimer >= searchDuration)
        {
            FinishSearching();
            return;
        }

        agent.speed = searchSpeed;
        agent.isStopped = false;

        if (agent.pathPending)
            return;

        // Investigate the player's last
        // known location first.
        if (!agent.hasPath ||
            agent.remainingDistance <=
            agent.stoppingDistance + 0.3f ||
            agent.pathStatus ==
            NavMeshPathStatus.PathInvalid)
        {
            searchingLastKnownPosition = false;

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
    // SEARCH DECISION
    // ------------------------------------

    void FinishSearching()
    {
        // If the timer requested a deck
        // change during the chase, choose
        // whether to leave or keep patrolling.
        if (IsDeckChangePending())
        {
            float randomDecision =
                Random.value;

            if (randomDecision <
                leaveDeckAfterSearchChance)
            {
                Debug.Log(
                    "Monster finished searching " +
                    "and decided to change decks."
                );

                ChangeDeckAfterPatrol();
                return;
            }
        }

        // Otherwise return to the patrol route.
        currentState = MonsterState.Patrol;
        waitTimer = 0f;

        Debug.Log(
            "Monster finished searching " +
            "and returned to patrol."
        );

        GoToCurrentPatrolPoint();
    }

    // ------------------------------------
    // DECK CHANGE DECISION
    // ------------------------------------

    bool IsDeckChangePending()
    {
        if (DeckStatusManager.Instance == null)
            return false;

        return DeckStatusManager.Instance
            .IsDeckChangePending();
    }

    void ChangeDeckAfterPatrol()
    {
        if (DeckStatusManager.Instance == null)
            return;

        if (!IsDeckChangePending())
            return;

        // The manager changes deck assignment.
        DeckStatusManager.Instance
            .CompleteDeckChange();

        // Hide the monster if its assignment
        // changed to the other deck.
        SyncWithDeckManager();
    }

    // ------------------------------------
    // VISION SYSTEM
    // ------------------------------------

    void CheckLineOfSight()
    {
        canSeePlayer = false;

        if (player == null)
            return;

        // Respect the hiding spot system.
        if (HidingSpotBehaviour.IsPlayerHidden(player))
        {
            if (!HidingSpotBehaviour
                .CanMonsterSeePlayer(player))
            {
                return;
            }
        }

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
    // VISION GIZMOS
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

    // ------------------------------------
    // CLEANUP
    // ------------------------------------

    void OnDestroy()
    {
        if (DeckStatusManager.Instance != null)
        {
            DeckStatusManager.Instance
                .UnregisterMonster(thisDeck, this);
        }
    }
}
