using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnBehaviour : MonoBehaviour
{
    public static RespawnBehaviour instance;
    public GameObject container;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Awake()
    {
        instance = this;
    }

    public void EnableDeathScreen()
    {
        container.SetActive(true);
    }

    public void RestartScreen()
    {
        SceneManager.LoadScene("LabDeck2");
    }
}
