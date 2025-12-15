using UnityEngine;

public class StartingRoom : DungeonRoom
{
    [Header("Interior/Furniture")]
    [SerializeField] private GameObject bedPrefab;

    protected override void GenerateInterior(Room room)
    {
        GenerateBeds(room, 0);
    }

    private void GenerateBeds(Room room, int iteration)
    {
        if (iteration > maxPlacementIterations)
        {
            Debug.LogError("Fail safe activated");
            return;
        }

        if (bedPrefab == null)
        {
            Debug.LogError("Bed prefabs are missing");
            return;
        }

        Vector2Int bedGridSize = new Vector2Int(2, 3);
        Vector3 bedPivotOffset = new Vector3(0, 0, 0);

        if (!TryPlaceObjectInCorner(room, bedPrefab, bedGridSize, bedPivotOffset))
        {
            GenerateBeds(room, ++iteration);
        }
    }
}
