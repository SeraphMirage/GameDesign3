using UnityEngine;

public class MonsterKillBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerDeathBehaviour playerDeath = other.GetComponentInParent<PlayerDeathBehaviour>();

        if (playerDeath != null)
        {
            playerDeath.Die();
        }
        else
        {
            Debug.LogError("PlayerDeath script not found on the Player!");
        }
    }
}
