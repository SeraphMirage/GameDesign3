using UnityEngine;
using UnityEngine.Events;

public class ButtonBehaviour : MonoBehaviour
{
    public Vector3 pressOffest = new Vector3(0, 0, 0);
    public UnityEvent onClick;

    public void PushDown()
    {
        transform.position += pressOffest;
        onClick.Invoke();
    }
}
