using UnityEngine;
using System.Collections.Generic;

public class EndlessRoad : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject roadPrefab;
    public Transform player;

    [Header("Yol Ayarlar?")]
    public float roadLength = 100f;
    public int numberOfTilesOnScreen = 3;

    private float spawnZ = 0f;
    private readonly List<GameObject> activeRoads = new List<GameObject>();

    void Start()
    {
        if (roadPrefab == null)
        {
            enabled = false;
            return;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        spawnZ = -roadLength;
        for (int i = 0; i < numberOfTilesOnScreen; i++)
        {
            SpawnRoad();
        }
    }

    void Update()
    {
        if (player == null || roadPrefab == null)
        {
            return;
        }

        if (player.position.z - roadLength > spawnZ - (numberOfTilesOnScreen * roadLength))
        {
            SpawnRoad();
            DeleteOldestRoad();
        }
    }

    void SpawnRoad()
    {
        if (roadPrefab == null)
        {
            return;
        }

        GameObject road = Instantiate(roadPrefab, new Vector3(0f, 0f, spawnZ + (roadLength / 2f)), Quaternion.identity, transform);
        activeRoads.Add(road);
        spawnZ += roadLength;
    }

    void DeleteOldestRoad()
    {
        if (activeRoads.Count == 0)
        {
            return;
        }

        if (activeRoads[0] != null)
        {
            Destroy(activeRoads[0]);
        }

        activeRoads.RemoveAt(0);
    }
}
