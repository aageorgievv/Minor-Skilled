using System.Collections.Generic;
using UnityEngine;

public class BedroomRoom : DungeonRoom
{
    [Header("Interior/Furniture")]
    [SerializeField] private GameObject[] tablePrefabs;
    [SerializeField] private GameObject[] bedPrefabs;
    [SerializeField] private GameObject chairPrefab;

    [Header("Treasure/Boxes")]
    [SerializeField] private GameObject[] treasureandBoxesPrefabs;

    [Header("Generation settings")]
    [SerializeField] private int tableSizeSpawn;
    [SerializeField] private int chestSizeSpawn;
    [SerializeField] private int bedSizeSpawn;
    [SerializeField, Range(5, 15)] private int maxChestSpawnAmount;

    protected override void GenerateInterior(Room room)
    {
        GenerateTableAndChairs(room, 0);
        GenerateBeds(room, 0);
        GenerateChests(room);
    }

    private void GenerateTableAndChairs(Room room, int iteration)
    {
        if (iteration > maxPlacementIterations)
        {
            Debug.LogError("Fail safe activated");
            return;
        }

        if (tablePrefabs.Length == 0)
        {
            Debug.LogError("table prefabs are missing!");
            return;
        }

        GameObject tablePrefab = tablePrefabs[Random.Range(0, tablePrefabs.Length)];

        Vector2Int tableGridSize = new Vector2Int(3, 3);
        Vector3 tablePivotOffset = Vector3.zero;
        int padding = 1;

        Vector2Int tableGridPos;
        Quaternion tableRotation;

        if (!TryPlaceObjectInCenterArea(room, tablePrefab, tableGridSize, tablePivotOffset, padding, out tableGridPos, out tableRotation))
        {
            GenerateTableAndChairs(room, ++iteration);
        }
        else
        {
            Vector2Int chairGridSize = new Vector2Int(1, 1);
            Vector3 chairPivotOffset = Vector3.zero;
            Vector2Int rotatedTableSize = GetRotatedSize(tableGridSize, tableRotation);

            int midX = tableGridPos.x + rotatedTableSize.x / 2;
            int midY = tableGridPos.y + rotatedTableSize.y / 2;

            Vector2Int bottomChairPos = new Vector2Int(midX, tableGridPos.y - 1);
            TryPlaceObjectOnGrid(room, chairPrefab, chairGridSize, bottomChairPos, Quaternion.Euler(0, -90, 0), chairPivotOffset);

            Vector2Int topChairPos = new Vector2Int(midX, tableGridPos.y + rotatedTableSize.y);
            TryPlaceObjectOnGrid(room, chairPrefab, chairGridSize, topChairPos, Quaternion.Euler(0, 90, 0), chairPivotOffset);

            Vector2Int leftChairPos = new Vector2Int(tableGridPos.x - 1, midY);
            TryPlaceObjectOnGrid(room, chairPrefab, chairGridSize, leftChairPos, Quaternion.Euler(0, 0, 0), chairPivotOffset);

            Vector2Int rightChairPos = new Vector2Int(tableGridPos.x + rotatedTableSize.x, midY);
            TryPlaceObjectOnGrid(room, chairPrefab, chairGridSize, rightChairPos, Quaternion.Euler(0, -180, 0), chairPivotOffset);
        }
    }

    private void GenerateBeds(Room room, int iteration)
    {
        if (iteration > maxPlacementIterations)
        {
            Debug.LogError("Fail safe activated");
            return;
        }

        if (bedPrefabs.Length == 0)
        {
            Debug.LogError("Bed prefabs are missing");
            return;
        }

        GameObject bedPrefab = bedPrefabs[Random.Range(0, bedPrefabs.Length)];
        Vector2Int bedGridSize = new Vector2Int(3, 3);
        Vector3 bedPivotOffset = new Vector3(0.5f, 0, 0);

        if (!TryPlaceObjectInCorner(room, bedPrefab, bedGridSize, bedPivotOffset))
        {
            GenerateBeds(room, ++iteration);
        }
    }

    private void GenerateChests(Room room)
    {
        if (treasureandBoxesPrefabs.Length == 0)
        {
            Debug.LogError("Treasure prefabs are missing");
            return;
        }

        int spawnAmount = Random.Range(0, maxChestSpawnAmount + 1);
        Vector2Int chestGridSize = new Vector2Int(2, 2);
        Vector3 chestPivotOffset = new Vector3(0, 0, -0.25f);

        for (int i = 0; i < spawnAmount; ++i)
        {
            GameObject randomPrefab = treasureandBoxesPrefabs[Random.Range(0, treasureandBoxesPrefabs.Length)];
            TryPlaceObjectAlongWall(room, randomPrefab, chestGridSize, chestPivotOffset, 0);
        }
    }
}
