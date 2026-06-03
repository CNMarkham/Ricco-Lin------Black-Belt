using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenrateLevels : MonoBehaviour

{
    [Header("Player")]
    public Transform player;

    [Header("Level Prefabs")]
    public GameObject[] levelPrefabs;

    [Header("Generation Settings")]
    public int tilesAhead = 10;
    public float tileLength = 20f;
    public float destroyDistance = 40f;

    private float nextSpawnZ = 1106f;
    private List<GameObject> spawnedTiles = new List<GameObject>();

    void Start()
    {
        // Spawn initial tiles
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
        GameObject prefab = levelPrefabs[Random.Range(0, levelPrefabs.Length)];

        Vector3 spawnPos = new Vector3(-24, -439.2121f, nextSpawnZ);

        GameObject tile = Instantiate(prefab, spawnPos, Quaternion.identity);

        spawnedTiles.Add(tile);

        nextSpawnZ += tileLength;
    }
}

