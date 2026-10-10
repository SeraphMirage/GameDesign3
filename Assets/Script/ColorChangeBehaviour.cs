using UnityEngine;

public class ColorChangeBehaviour : MonoBehaviour
{
    public Renderer buttonRenderer;
    public GameObject button;
    public Color newColor;

    private void Start()
    {
        buttonRenderer = button.GetComponent<Renderer>();
    }

    public void ChangeMaterial()
    {
        buttonRenderer.material.color = newColor;
    }
}
