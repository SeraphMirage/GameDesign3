using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{
    public Transform Player;

    public float chaseSpeed;
    public float aggroDistance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, Player.position);
        if (distance > aggroDistance) return;

        transform.position = Vector3.MoveTowards(transform.position, Player.position, chaseSpeed * Time.deltaTime);
    }
}
