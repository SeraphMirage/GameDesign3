using UnityEngine;
using UnityEngine.Events;

public class ButtonBehaviour : MonoBehaviour
{
    public Material redMaterial;
    public Material greenMaterial;

    private Renderer objectRenderer;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            // Force the object to start with the red material
            if (redMaterial != null)
            {
                objectRenderer.material = redMaterial;
            }
        }
        else
        {
            Debug.LogError("No Renderer found on " + gameObject.name);
        }
    }

    private void OnMouseDown()
    {
        if (objectRenderer != null && redMaterial != null && greenMaterial != null)
        {
            // Toggle between the two materials
            if (objectRenderer.sharedMaterial == greenMaterial)
            {
                objectRenderer.material = redMaterial;
            }
            else
            {
                objectRenderer.material = greenMaterial;
            }
        }
    }
}
