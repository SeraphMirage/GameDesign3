using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{
    public Transform Player;

    public float chaseSpeed = 3f;
    public float aggroDistance = 10f;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            Player = playerObject.transform;
        }
        else
        {
            Debug.LogError("No GameObject tagged 'Player' was found!", this);
        }
    }

    void Update()
    {
        // Prevent errors if the player hasn't been found
        if (Player == null) return;

        float distance = Vector3.Distance(transform.position, Player.position);

        if (distance > aggroDistance) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            Player.position,
            chaseSpeed * Time.deltaTime
        );
    }
}
