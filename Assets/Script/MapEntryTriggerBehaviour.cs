
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapEntryTrigger : MonoBehaviour
{
    [Header("Map Settings")]
    public string mapSceneName = "Ship UI";

    [Header("Interaction")]
    public float interactionDistance = 3f;

    // ------------------------------------
    // UPDATE
    // ------------------------------------

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;

            if (cam == null)
                return;

            Ray ray = new Ray(
                cam.transform.position,
                cam.transform.forward
            );

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactionDistance
            ))
            {
                if (hit.transform == transform)
                {
                    OpenShipMap();
                }
            }
        }
    }

    // ------------------------------------
    // OPEN SHIP MAP
    // ------------------------------------

    void OpenShipMap()
    {
        // Automatically detect which deck
        // the player is currently exploring.
        string currentScene =
            SceneManager.GetActiveScene().name;

        // Save the player's current deck.
        PlayerPrefs.SetString(
            "MapReturnScene",
            currentScene
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Opening Ship Map from: " +
            currentScene
        );

        // Open the Ship Map scene.
        SceneManager.LoadScene(mapSceneName);
    }
}
