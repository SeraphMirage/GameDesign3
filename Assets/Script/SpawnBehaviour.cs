using UnityEngine;

public class SpawnBehaviour : MonoBehaviour
{
    public GameObject prefabToSpawn;

    [SerializeField] private Transform spawnLocation;
    [SerializeField] private bool triggerOnlyOnce = true;

    // NEW
    [SerializeField] private bool spawnOnStart = false;

    private bool hasTriggered = false;

    private void Start()
    {
        // Spawn immediately when the game begins
        if (spawnOnStart)
        {
            SpawnObject();

            if (triggerOnlyOnce)
            {
                hasTriggered = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (triggerOnlyOnce && hasTriggered)
                return;

            SpawnObject();
            hasTriggered = true;
        }
    }

    private void SpawnObject()
    {
        if (prefabToSpawn != null)
        {
            Vector3 spawnPos =
                spawnLocation != null
                ? spawnLocation.position
                : transform.position;

            Quaternion spawnRot =
                spawnLocation != null
                ? spawnLocation.rotation
                : Quaternion.identity;

            Instantiate(
                prefabToSpawn,
                spawnPos,
                spawnRot
            );
        }
        else
        {
            Debug.LogWarning(
                "SpawnBehaviour has no prefab assigned!",
                this
            );
        }
    }
}