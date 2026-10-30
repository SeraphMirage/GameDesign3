using UnityEngine;

using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipMapUI : MonoBehaviour
{
    public GameObject mapPanel;

    private bool mapOpen = false;

    void Start()
    {
        if (mapPanel != null)
        {
            mapPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (mapOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMap();
        }
    }

    public void OpenMap()
    {
        mapOpen = true;

        if (mapPanel != null)
        {
            mapPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseMap()
    {
        mapOpen = false;

        if (mapPanel != null)
        {
            mapPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GoToUpperDeck()
    {
        SceneManager.LoadScene("UpperDeck");
    }

    public void GoToLowerDeck()
    {
        SceneManager.LoadScene("LowerDeck");
    }
}
