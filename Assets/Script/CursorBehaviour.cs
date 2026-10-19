using UnityEngine;

public class CursorBehaviour : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnGUI()
    {
        float size = 10f;

        float x = (Screen.width - size) / 2;
        float y = (Screen.height - size) / 2;

        GUI.Label(new Rect(x, y, size, size), "+");
    }
}
