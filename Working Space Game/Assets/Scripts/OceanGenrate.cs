using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanGenrate : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Level Prefabs")]
    public GameObject[] levelPrefabs;

    [Header("Generation Settings")]
    public int tilesAhead = 10;
    public float tileLength = 20f;
    public float destroyDistance = 40f;

    private float nextSpawnZ = 450f;
    private List<GameObject> spawnedTiles = new List<GameObject>();
    private int tileCount = 0;
    private int checkpointTarget;
    public GameObject Checkpoint;
    void Start()
    {
        SetNewCheckpointTarget();

        for (int i = 0; i < tilesAhead; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {
        // Generate new tiles ahead of player
        while (player.position.z + (tilesAhead * tileLength) > nextSpawnZ)
        {
            SpawnTile();
        }

        // Remove old tiles behind player
        for (int i = spawnedTiles.Count - 1; i >= 0; i--)
        {
            if (player.position.z - spawnedTiles[i].transform.position.z > destroyDistance)
            {
                Destroy(spawnedTiles[i]);
                spawnedTiles.RemoveAt(i);
            }
        }
    }

    void SpawnTile()
    {
        //everytime we spwan a tile increase count
        //when it hits the count then spawn checkpoint
        //reset count and pick new number 4-10


        GameObject prefab = levelPrefabs[Random.Range(0, levelPrefabs.Length)];

        Vector3 spawnPos = new Vector3(0, 0, nextSpawnZ);

        GameObject tile = Instantiate(prefab, spawnPos, Quaternion.identity);

        tileCount++;

        spawnedTiles.Add(tile);

        nextSpawnZ += tileLength;

        if (tileCount >= checkpointTarget)
        {
            SpawnCheckpoint();

            tileCount = 0;
            SetNewCheckpointTarget();
        }
    }

    private void SetNewCheckpointTarget()
    {
        checkpointTarget = Random.Range(4, 7);
    }

    public void SpawnCheckpoint()
    {

        Vector3 spawnPos = new Vector3(0, 0f, nextSpawnZ);

        GameObject newObject = Instantiate(Checkpoint, spawnPos, Quaternion.identity);
    }
}
