using UnityEngine;

public class SpawnBehaviour : MonoBehaviour
{
    public GameObject prefabToSpawn;
    [SerializeField] private Transform spawnLocation;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            if (triggerOnlyOnce && hasTriggered) return;

            SpawnObject();
            hasTriggered = true;
        }
    }
    private void SpawnObject()
    {
        if (prefabToSpawn != null)
        {
            // If a custom spawn location is set, use it. Otherwise, spawn at this trigger's position.
            Vector3 spawnPos = spawnLocation != null ? spawnLocation.position : transform.position;
            Quaternion spawnRot = spawnLocation != null ? spawnLocation.rotation : Quaternion.identity;

            Instantiate(prefabToSpawn, spawnPos, spawnRot);
        }
    }
}
