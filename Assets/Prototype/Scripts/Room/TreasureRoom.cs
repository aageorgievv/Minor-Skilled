using UnityEngine;

public class TreasureRoom : DungeonRoom
{
    [Header("Treasure/Loot")]
    [SerializeField] private GameObject[] smallBoxesPrefabs;
    [SerializeField] private GameObject[] treasureObjectPrefabs;
    [SerializeField] private GameObject largeBoxPrefab;

    [Header("Generation settings")]
    [SerializeField, Range(3, 10)] private int maxChestSpawnAmount;
    [SerializeField, Range(5, 15)] private int coinStackSpawnAmount;

    protected override void GenerateInterior(Room room)
    {
        GenerateLargeTreasureBox(room, 0);
        GenerateChestsAndBoxes(room);
        GenerateCoinStacks(room);
    }
    private void GenerateLargeTreasureBox(Room room, int iteration)
    {
        if(largeBoxPrefab == null)
        {
            Debug.LogError("LargeBox prefab is missing");
            return;
        }

        Vector2Int boxGridSize = new Vector2Int(4, 4);
        Vector3 pivotOffest = Vector3.zero;

        if (!TryPlaceObjectInCorner(room, largeBoxPrefab, boxGridSize, pivotOffest))
        {
            GenerateLargeTreasureBox(room, ++iteration);
        }
    }

    private void GenerateChestsAndBoxes(Room room)
    {
        if (smallBoxesPrefabs.Length == 0)
        {
            Debug.LogError("Boxes prefabs are missing");
            return;
        }

        int chestSpawnAmount = Random.Range(0, maxChestSpawnAmount + 1);
        Vector2Int chestGridSize = new Vector2Int(2, 2);
        Vector3 chestPivotOffset = new Vector3(0, 0, -0.25f);

        for (int i = 0; i < chestSpawnAmount; ++i)
        {
            GameObject randomPrefab = smallBoxesPrefabs[Random.Range(0, smallBoxesPrefabs.Length)];
            TryPlaceObjectAlongWall(room, randomPrefab, chestGridSize, chestPivotOffset, 0);
        }
    }

    private void GenerateCoinStacks(Room room)
    {
        if(treasureObjectPrefabs.Length == 0)
        {
            Debug.LogError("Treasure prefabs are missing");
            return;
        }

        
        int spawnAmount = Random.Range(0, coinStackSpawnAmount);
        Vector2Int gridSize = new Vector2Int(2, 2);
        Vector3 pivotOffest = Vector3.zero;

        for (int i = 0; i < spawnAmount; ++i)
        {
            GameObject randomPrefab = treasureObjectPrefabs[Random.Range(0, treasureObjectPrefabs.Length)];
            TryPlaceObjectInCenterArea(room, randomPrefab, gridSize, pivotOffest, 0);
        }
    }
}
