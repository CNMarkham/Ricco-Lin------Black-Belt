using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class finallevelspawner : MonoBehaviour
{
    [Header("Spawn Menu / Prefabs")]
    [Tooltip("Drag and drop your object prefabs here in the Inspector menu.")]
    public GameObject[] objectPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Maximum number of objects allowed at the same time.")]
    public int maxObjects = 3;

    [Tooltip("Distance in front of the player where objects will spawn.")]
    public float spawnDistanceAhead = 20f;

    [Header("Despawn Settings")]
    [Tooltip("Assign your Player's Transform here in the Inspector.")]
    public Transform player;

    [Tooltip("Distance behind the player at which objects will despawn.")]
    public float despawnDistanceBehind = 40f;

    [Header("X and Y Axis Parameters (Offsets relative to player view)")]
    [Tooltip("Minimum horizontal offset (Left/Right) from the spawn point.")]
    public float minX = -15f;
    [Tooltip("Maximum horizontal offset (Left/Right) from the spawn point.")]
    public float maxX = 15f;
    [Tooltip("Minimum vertical offset (Up/Down) from the spawn point.")]
    public float minY = 0f;
    [Tooltip("Maximum vertical offset (Up/Down) from the spawn point.")]
    public float maxY = 15f;

    // Tracks currently spawned objects to enforce the limit
    private List<GameObject> activeObjects = new List<GameObject>();

    void Update()
    {
        // Clean up any references to objects that were destroyed or deleted
        activeObjects.RemoveAll(obj => obj == null);

        // If the player transform is assigned, handle tracking and despawning
        if (player != null)
        {
            // Check if any active object is 40 units BEHIND the player
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                if (activeObjects[i] != null)
                {
                    Vector3 toObject = activeObjects[i].transform.position - player.position;

                    // The dot product with player.forward tells us how far ahead (+) or behind (-) the object is
                    float forwardDistance = Vector3.Dot(player.forward, toObject);

                    // If it's behind the player by 40 units or more (negative value <= -40)
                    if (forwardDistance <= -despawnDistanceBehind)
                    {
                        Destroy(activeObjects[i]);
                        activeObjects.RemoveAt(i);
                    }
                }
            }
        }

        // Spawn a new object only if we are below the limit of 3 and player is assigned
        if (activeObjects.Count < maxObjects && player != null)
        {
            yield new WaitForSeconds(despawnDistanceBehind);
            SpawnRandomObject();
        }
    }

    void SpawnRandomObject()
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0 || player == null)
        {
            return;
        }

        // 1. Pick a random prefab from the array
        GameObject selectedPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];

        // 2. Generate random X and Y offsets within your parameters
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        // 3. Calculate position: 20 units ahead of player, adjusted by player's right/up vectors for X & Y offsets
        Vector3 spawnPosition = player.position
                              + (player.forward * spawnDistanceAhead)
                              + (player.right * randomX)
                              + (player.up * randomY);

        // 4. Instantiate the object and add it to our tracking list
        GameObject spawnedObj = Instantiate(selectedPrefab, spawnPosition, new Quaternion(-0.706983387f, 0, 0, 0.70723021f));
        activeObjects.Add(spawnedObj);
    }
}