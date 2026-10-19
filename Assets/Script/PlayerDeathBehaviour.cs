using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathBehaviour : MonoBehaviour
{
    private bool isDead = false;

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        // Load the death menu scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("DeathScreen");
    }
}
