using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipMapUIBehaviour : MonoBehaviour
{
    void Awake()
    {
        ShowMouse();
    }

    void Start()
    {
        ShowMouse();
    }

    void Update()
    {
        ShowMouse();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMap();
        }
    }

    void LateUpdate()
    {
        ShowMouse();
    }

    private void ShowMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToTopDeck()
    {
        SceneManager.LoadScene("Top Deck");
    }

    public void GoToBottomDeck()
    {
        SceneManager.LoadScene("LabDeck2");
    }

    public void ExitMap()
    {
        string returnScene = PlayerPrefs.GetString(
            "MapReturnScene",
            "Top Deck"
        );

        SceneManager.LoadScene(returnScene);
    }
}