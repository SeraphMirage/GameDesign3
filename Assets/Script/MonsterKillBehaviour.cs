
using UnityEngine;

public class MonsterKillBehaviour : MonoBehaviour
{
    [Header("Kill Settings")]
    public bool instantKill = true;

    private bool hasKilledPlayer = false;

    // ------------------------------------
    // ENABLE
    // ------------------------------------

    void OnEnable()
    {
        hasKilledPlayer = false;
    }

    // ------------------------------------
    // TRIGGER CONTACT
    // ------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        TryKillPlayer(other);
    }

    // Detect player if already touching
    // the monster's trigger.
    private void OnTriggerStay(Collider other)
    {
        TryKillPlayer(other);
    }

    // ------------------------------------
    // PHYSICAL COLLISION
    // ------------------------------------

    private void OnCollisionEnter(Collision collision)
    {
        TryKillPlayer(collision.collider);
    }

    // ------------------------------------
    // KILL PLAYER
    // ------------------------------------

    private void TryKillPlayer(Collider other)
    {
        if (!instantKill)
            return;

        if (hasKilledPlayer)
            return;

        if (other == null)
            return;

        // Find the player's death script,
        // including when touching child colliders.
        PlayerDeathBehaviour playerDeath =
            other.GetComponentInParent<PlayerDeathBehaviour>();

        if (playerDeath == null)
            return;

        // Make sure this is actually the player.
        if (!playerDeath.CompareTag("Player"))
            return;

        // Prevent repeated death calls.
        hasKilledPlayer = true;

        Debug.Log(
            "Monster caught the Player! Instant Kill!"
        );

        // Let PlayerDeathBehaviour handle
        // loading the Kill Scene.
        playerDeath.Die();
    }

    // ------------------------------------
    // RESET
    // ------------------------------------

    public void ResetKill()
    {
        hasKilledPlayer = false;
    }
}
