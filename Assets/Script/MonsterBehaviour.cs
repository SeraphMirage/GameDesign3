
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
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

    private NavMeshAgent agent;

    private Transform[] patrolPoints;
    private int currentPatrolPoint;
    private float waitTimer;

    private bool canSeePlayer;
    private bool monsterActive;
    private bool lastDeckMatch;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        FindPlayer();

        // Start with the monster hidden until
        // the deck manager confirms its location.
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

            if (player == null)
                return;
        }

        if (!agent.isOnNavMesh)
            return;

        CheckLineOfSight();

        if (canSeePlayer)
            ChasePlayer();
        else
            Patrol();
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
            player = playerObject.transform;
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

        lastDeckMatch = deckMatches;
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

        // Collect valid spawn locations.
        List<int> validIndices = new List<int>();
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

            if (NavMesh.SamplePosition(
                location.spawnPoint.position,
                out NavMeshHit hit,
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

        // Pick a random valid location.
        int choice =
            DeckStatusManager.Instance
            .GetRandomSpawnIndex(
                thisDeck,
                validIndices.Count
            );

        int selectedIndex = validIndices[choice];

        MonsterSpawnLocation selected =
            spawnLocations[selectedIndex];

        Vector3 spawnPosition =
            validPositions[choice];

        // Enable the NavMeshAgent before warping.
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

        patrolPoints = selected.patrolPoints;

        currentPatrolPoint = 0;
        waitTimer = 0f;
        canSeePlayer = false;

        SetMonsterActive(true);

        GoToCurrentPatrolPoint();

        Debug.Log(
            "Monster spawned on " +
            thisDeck +
            " at Spawn Point " +
            (selectedIndex + 1)
        );
    }

    // ------------------------------------
    // VISIBILITY AND MOVEMENT
    // ------------------------------------

    void SetMonsterActive(bool active)
    {
        monsterActive = active;

        if (agent != null)
        {
            if (active)
            {
                agent.isStopped = false;
            }
            else
            {
                if (agent.isOnNavMesh)
                {
                    agent.ResetPath();
                    agent.isStopped = true;
                }
            }
        }

        // Hide or show monster renderers.
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            rend.enabled = active;
        }

        // Disable physical monster colliders
        // while it is on the other deck.
        Collider[] colliders =
            GetComponentsInChildren<Collider>();

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

        Transform point =
            patrolPoints[currentPatrolPoint];

        if (point == null)
            return;

        agent.speed = patrolSpeed;
        agent.isStopped = false;

        agent.SetDestination(point.position);
    }

    // ------------------------------------
    // CHASE SYSTEM
    // ------------------------------------

    void ChasePlayer()
    {
        if (player == null)
            return;

        agent.speed = chaseSpeed;
        agent.isStopped = false;

        agent.SetDestination(player.position);
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
