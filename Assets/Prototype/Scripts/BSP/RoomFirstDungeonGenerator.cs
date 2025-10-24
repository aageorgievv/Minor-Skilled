using System.Collections.Generic;
using UnityEngine;

public class RoomFirstDungeonGenerator : AbstractDungeonGenerator
{
    [Header("Settings")]
    [SerializeField] private int minXWidth;
    [SerializeField] private int minZWidth;
    [SerializeField] private int dungeonSizeX;
    [SerializeField] private int dungeonSizeZ;
    [SerializeField, Range(1, 5)] private int offset;

    private float roomScale = 4f;
    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        var roomsList = BinarySpacePartitioningAlgorithm.BinarySpacePartitioning(new BoundsInt(startPosition, new Vector3Int(dungeonSizeX, 0, dungeonSizeZ)), minXWidth, minZWidth);

        foreach (var roomBounds in roomsList)
        {
            //Debug.Log($"Room bounds: min({roomBounds.min.x},{roomBounds.min.z}) size({roomBounds.size.x},{roomBounds.size.z})");
            SpawnRoom(roomBounds);
        }
    }

    private void SpawnRoom(BoundsInt roomBounds)
    {
        Vector3 roomCenter = roomBounds.center;
        Vector3 worldCenter = roomCenter * roomScale;
        Vector2 roomSize = new Vector2(roomBounds.size.x * roomScale, roomBounds.size.z * roomScale);

        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            Debug.LogError("No room prefabs assigned in the generator!");
            return;
        }

        DungeonRoom randomRoomPrefab = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Length)];

        DungeonRoom newRoom = Instantiate(randomRoomPrefab, worldCenter, Quaternion.identity, transform);
        newRoom.size = roomSize;
        newRoom.StartGenerating();
    }
}
