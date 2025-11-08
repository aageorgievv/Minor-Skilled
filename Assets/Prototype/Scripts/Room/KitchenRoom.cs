using UnityEngine;

public class KitchenRoom : DungeonRoom
{
    [Header("Interior/Furniture")]
    [SerializeField] private GameObject barPresetPrefab;
    [SerializeField] private GameObject[] barrelPrefabs;
    [SerializeField] private GameObject[] longTablePrefabs;
    [SerializeField] private GameObject chairPrefab;

    [Header("Generation settings")]
    [SerializeField] private int barSizeSpawn;
    [SerializeField] private int tableSizeSpawn;
    [SerializeField] private int barrelSizeSpawn;
    [SerializeField, Range(1, 10)] private int barrelSpawnAmount;



    protected override void GenerateInterior(Room room)
    {
        GenerateBar(room, 0);
        GenerateTableAndChairs(room, 0);
        GenerateBarrels(room);
    }

    private void GenerateTableAndChairs(Room room, int iteration)
    {
        if (iteration > maxPlacementIterations)
        {
            Debug.LogError("Fail safe activated");
            return;
        }

        if (longTablePrefabs.Length == 0)
        {
            Debug.LogError("table prefabs are missing!");
            return;
        }

        if (room.Width >= tableSizeSpawn && room.Length >= tableSizeSpawn)
        {
            GameObject tablePrefab = longTablePrefabs[Random.Range(0, longTablePrefabs.Length)];

            Vector2Int tableGridSize = new Vector2Int(3, 4);
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
    }

    private void GenerateBar(Room room, int iteration)
    {
        if (iteration > maxPlacementIterations)
        {
            Debug.LogError("Fail safe activated");
            return;
        }

        if (this.barPresetPrefab == null)
        {
            Debug.LogError("Bar prefab are missing");
            return;
        }

        if (room.Width >= barSizeSpawn && room.Length >= barSizeSpawn)
        {
            Vector2Int barGridSize = new Vector2Int(14, 7);
            Vector3 barPivotOffset = new Vector3(0, 1f, -3.7f);

            TryPlaceObjectAlongWall(room, barPresetPrefab, barGridSize, barPivotOffset, 0);
        }
    }

    private void GenerateBarrels(Room room)
    {
        if (this.barrelPrefabs == null)
        {
            Debug.LogError("Barrels prefabs are missing");
            return;
        }

        if (room.Width >= barrelSizeSpawn && room.Length >= barrelSizeSpawn)
        {
            GameObject barrelPrefabs = this.barrelPrefabs[Random.Range(0, this.barrelPrefabs.Length)];
            int barrelSpawnAmount = Random.Range(0, this.barrelSpawnAmount);
            Vector2Int barrelGridSize = new Vector2Int(2, 2);
            Vector3 barrelPivotOffset = Vector3.zero;

            for (int i = 0; i < barrelSpawnAmount; ++i)
            {
                TryPlaceObjectAlongWall(room, barrelPrefabs, barrelGridSize, barrelPivotOffset, 0);
            }
        }
    }
}
