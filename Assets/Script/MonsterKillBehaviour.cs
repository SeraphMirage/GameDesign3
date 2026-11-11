
using UnityEngine;

public class MonsterKillBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerDeathBehaviour playerDeath =
            other.GetComponentInParent<PlayerDeathBehaviour>();

        if (playerDeath == null)
            return;

        if (!playerDeath.CompareTag("Player"))
            return;

        Debug.Log("Monster caught the Player!");

        playerDeath.Die();
    }
}
