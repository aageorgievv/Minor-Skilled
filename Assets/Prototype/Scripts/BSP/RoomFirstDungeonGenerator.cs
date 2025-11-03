using System.Collections.Generic;
using UnityEngine;

public class RoomFirstDungeonGenerator : AbstractDungeonGenerator
{
    [Header("Settings")]
    [SerializeField] private int minXWidth;
    [SerializeField] private int minZWidth;
    [SerializeField] private int dungeonSizeX;
    [SerializeField] private int dungeonSizeZ;
    [SerializeField, Range(0, 2)] private int offset;

    private float roomScale = 4f;
    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        int minPartitionX = minXWidth + (offset * 2);
        int minPartitionZ = minZWidth + (offset * 2);
        var roomsList = BinarySpacePartitioningAlgorithm.BinarySpacePartitioning(new BoundsInt(startPosition, new Vector3Int(dungeonSizeX, 0, dungeonSizeZ)), minPartitionX, minPartitionZ);

        foreach (var roomBounds in roomsList)
        {
            Vector3Int newMin = new Vector3Int(
                roomBounds.min.x + offset,
                roomBounds.min.y,
                roomBounds.min.z + offset);

            Vector3Int newSize = new Vector3Int(
                roomBounds.size.x - offset,
                roomBounds.size.y,
                roomBounds.size.z - offset);

            BoundsInt newBounds = new BoundsInt(newMin, newSize);
            SpawnRoom(newBounds);
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
        //newRoom.size = roomSize; //no longer setting the size with the 1/2 unit change
        newRoom.StartGenerating();
    }
}
