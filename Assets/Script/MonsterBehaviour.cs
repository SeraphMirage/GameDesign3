
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
    public float sightDistance = 22f;

    [Range(0f, 360f)]
    public float fieldOfView = 110f;

    public float eyeHeight = 1f;

    [Header("Search")]
    public float searchDuration = 15f;
    public float searchSpeed = 2.5f;
    public float maxTravelTime = 12f;
    public float searchRadius = 12f;
    public float searchPointWaitTime = 1f;

    private NavMeshAgent agent;

    private Transform[] patrolPoints;
    private int currentPatrolPoint = 0;
    private float waitTimer = 0f;

    private bool canSeePlayer = false;
    private string currentDeck = "";

    private Vector3 lastKnownPlayerPosition;
    private float searchTimer = 0f;
    private float travelTimer = 0f;
    private float searchPointWaitTimer = 0f;

    private bool reachedSearchLocation = false;
    private bool waitingAtSearchPoint = false;

    private enum MonsterState
    {
        Patrol,
        Chase,
        Search
    }

    private MonsterState currentState = MonsterState.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        SyncWithDeckManager();
    }

    void Update()
    {
        SyncWithDeckManager();

        if (player == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (currentDeck == "")
            return;

        CheckLineOfSight();

        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.position;

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

    void SyncWithDeckManager()
    {
        if (DeckStatusManager.Instance == null)
            return;

        string assignedDeck =
            DeckStatusManager.Instance.monsterDeck;

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
            return;
        }

        if (destination == null)
            return;

        NavMeshHit hit;

        if (!NavMesh.SamplePosition(
            destination.position,
            out hit,
            1.5f,
            NavMesh.AllAreas))
        {
            return;
        }

        agent.ResetPath();

        if (!agent.Warp(hit.position))
            return;

        currentDeck = newDeck;

        currentPatrolPoint = 0;
        waitTimer = 0f;
        canSeePlayer = false;

        currentState = MonsterState.Patrol;
        searchTimer = 0f;
        travelTimer = 0f;
        searchPointWaitTimer = 0f;

        reachedSearchLocation = false;
        waitingAtSearchPoint = false;

        lastKnownPlayerPosition = Vector3.zero;

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

        if (!IsPlayerOnCurrentDeck())
        {
            canSeePlayer = false;
            currentState = MonsterState.Patrol;
            GoToCurrentPatrolPoint();
            return;
        }

        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void StartSearching()
    {
        currentState = MonsterState.Search;

        searchTimer = searchDuration;
        travelTimer = 0f;
        searchPointWaitTimer = 0f;

        reachedSearchLocation = false;
        waitingAtSearchPoint = false;

        agent.speed = searchSpeed;
        agent.SetDestination(lastKnownPlayerPosition);
    }

    void SearchForPlayer()
    {
        agent.speed = searchSpeed;

        // Count down the total search time.
        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            EndSearch();
            return;
        }

        // First investigate the last known position.
        if (!reachedSearchLocation)
        {
            travelTimer += Time.deltaTime;

            if (agent.pathPending)
                return;

            if (agent.pathStatus ==
                NavMeshPathStatus.PathInvalid ||
                agent.pathStatus ==
                NavMeshPathStatus.PathPartial ||
                agent.remainingDistance <=
                agent.stoppingDistance ||
                travelTimer >= maxTravelTime)
            {
                reachedSearchLocation = true;
                travelTimer = 0f;

                PickSearchPoint();
            }

            return;
        }

        // Search nearby rooms and hallways.
        if (agent.pathPending)
            return;

        if (!waitingAtSearchPoint)
        {
            travelTimer += Time.deltaTime;

            if (!agent.hasPath ||
                agent.remainingDistance <=
                agent.stoppingDistance ||
                travelTimer >= maxTravelTime)
            {
                waitingAtSearchPoint = true;
                searchPointWaitTimer = 0f;

                agent.ResetPath();
            }
        }
        else
        {
            searchPointWaitTimer += Time.deltaTime;

            if (searchPointWaitTimer >=
                searchPointWaitTime)
            {
                PickSearchPoint();
            }
        }
    }

    void PickSearchPoint()
    {
        waitingAtSearchPoint = false;
        travelTimer = 0f;
        searchPointWaitTimer = 0f;

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection =
                Random.insideUnitCircle * searchRadius;

            Vector3 randomPoint =
                lastKnownPlayerPosition +
                new Vector3(
                    randomDirection.x,
                    0f,
                    randomDirection.y
                );

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                randomPoint,
                out hit,
                2f,
                NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();

                if (agent.CalculatePath(
                    hit.position,
                    path) &&
                    path.status ==
                    NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }

        // Try again after a short pause.
        waitingAtSearchPoint = true;
    }

    void EndSearch()
    {
        currentState = MonsterState.Patrol;

        searchTimer = 0f;
        travelTimer = 0f;
        searchPointWaitTimer = 0f;

        reachedSearchLocation = false;
        waitingAtSearchPoint = false;

        waitTimer = 0f;

        agent.ResetPath();
        GoToCurrentPatrolPoint();
    }

    void CheckLineOfSight()
    {
        canSeePlayer = false;

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