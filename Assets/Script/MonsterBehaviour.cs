using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{
    public Transform Player;

    public float chaseSpeed = 3f;
    public float aggroDistance = 10f;
    public float spawnGracePeriod = 1f;

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            Player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Monster couldn't find the Player!", this);
        }
    }

    void Update()
    {
        if (Player == null) return;

        // Chase the Player on spawn
        transform.position = Vector3.MoveTowards(
            transform.position,
            Player.position,
            chaseSpeed * Time.deltaTime
        );

        if (Time.time - spawnTime < spawnGracePeriod) return;

        float distance = Vector3.Distance(
            transform.position,
            Player.position
        );

        // Disappear if the Player gets too far away.
        if (distance > aggroDistance)
        {
            Destroy(gameObject);
        }
    }

}
