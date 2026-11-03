using UnityEngine;
using UnityEngine.SceneManagement;

public class MapEntryTrigger : MonoBehaviour
{
    public string mapSceneName = "Ship UI";
    public string returnSceneName = "Top Deck";
    public float interactionDistance = 3f;

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
                    PlayerPrefs.SetString(
                        "MapReturnScene",
                        returnSceneName
                    );

                    SceneManager.LoadScene(mapSceneName);
                }
            }
        }
    }
}