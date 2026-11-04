using UnityEngine;

public class TempRepairBehaviour : MonoBehaviour
{
    public TempControlBehaviour temperatureSystem;

    private void OnMouseDown()
    {
        if (temperatureSystem != null)
        {
            temperatureSystem.RepairSystem();
        }
    }
}
