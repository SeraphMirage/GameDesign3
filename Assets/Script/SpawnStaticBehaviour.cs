using UnityEngine;

public class SpawnStaticBehaviour : MonoBehaviour
{
    public GameObject prefabToSpawn;

    [SerializeField] private Transform spawnLocation;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;
    private GameObject spawnedObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (triggerOnlyOnce && hasTriggered) return;

            SpawnObject();
            hasTriggered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
                spawnedObject = null;
            }
        }
    }

    private void SpawnObject()
    {
        if (prefabToSpawn != null)
        {
            Vector3 spawnPos = spawnLocation != null
                ? spawnLocation.position
                : transform.position;

            Quaternion spawnRot = spawnLocation != null
                ? spawnLocation.rotation
                : Quaternion.identity;

            spawnedObject = Instantiate(prefabToSpawn, spawnPos, spawnRot);
        }
        else
        {
            Debug.LogError("No prefab assigned to the spawner!", this);
        }
    }
}
