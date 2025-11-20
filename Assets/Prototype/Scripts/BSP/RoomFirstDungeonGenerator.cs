using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class RoomFirstDungeonGenerator : AbstractDungeonGenerator
{
    [Header("Settings")]
    [SerializeField] private int minXWidth;
    [SerializeField] private int minZWidth;
    [SerializeField] private int dungeonSizeX;
    [SerializeField] private int dungeonSizeZ;

    [Header("Corridor Settings")]
    [SerializeField] private int corridorWidth = 2;
    [SerializeField] private CorridorRoom corridorPrefab;

    private Dictionary<Vector2Int, BoundsInt> roomBoundsMap = new Dictionary<Vector2Int, BoundsInt>();

    private float roomScale = 4f;
    const int padding = 2;

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        roomBoundsMap.Clear();

        int minPartitionX = minXWidth + (padding * 2);
        int minPartitionZ = minZWidth + (padding * 2);

        BoundsInt dungeonBounds = new BoundsInt(startPosition, new Vector3Int(dungeonSizeX, 1, dungeonSizeZ));

        var roomsList = BinarySpacePartitioningAlgorithm.BinarySpacePartitioning(dungeonBounds, minPartitionX, minPartitionZ);

        List<BoundsInt> roomCenters = new List<BoundsInt>();

        foreach (var roomBounds in roomsList)
        {
            Vector3Int newMin = new Vector3Int(
                roomBounds.min.x + padding * 2,
                roomBounds.min.y,
                roomBounds.min.z + padding * 2);

            Vector3Int newSize = new Vector3Int(
                roomBounds.size.x - (padding * 2),
                roomBounds.size.y,
                roomBounds.size.z - padding * 2);

            if (newSize.x <= 0 || newSize.z <= 0)
            {
                continue;
            }

            BoundsInt newBounds = new BoundsInt(newMin, newSize);
            //Vector3Int center3D = Vector3Int.RoundToInt(newBounds.center);
            //Vector2Int center2D = new Vector2Int(center3D.x, center3D.z);

            SpawnRoom(newBounds);
            roomCenters.Add(newBounds);

            //roomBoundsMap.Add(center2D, newBounds);
        }

        ConnectRooms(roomCenters);
    }

    private void ConnectRooms(List<BoundsInt> roomCenters)
    {
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            BoundsInt closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            SpawnCorridorConnection(currentRoomCenter, closest);
            currentRoomCenter = closest;
        }
    }

    private void SpawnCorridorConnection(BoundsInt startRoom, BoundsInt endRoom)
    {
        //Horizontal
        if(startRoom.x != endRoom.x)
        {
            int corridorStartX;
            int corridorEndX;

            if(startRoom.x < endRoom.x)
            {
                corridorStartX = startRoom.max.x + padding;
                corridorEndX = endRoom.min.x - padding;
            }
            else
            {
                corridorStartX = endRoom.max.x + padding; // accounting for the walls
                corridorEndX = startRoom.min.x - padding;
            }

            int x = corridorStartX + (corridorEndX - corridorStartX) / 2;
            int lengthX = corridorEndX - corridorStartX + padding * 2; // accounting for the padding on both walls

            int corridorZ = Mathf.RoundToInt(startRoom.center.z);
            Vector3 position = new Vector3(x, 0, corridorZ - corridorWidth / 2);
            SpawnCorridor(position, new Vector2Int(lengthX * 2, corridorWidth), CorridorRoom.EOrientation.Horizontal);
        }
    }
    
    private void SpawnCorridor(Vector3 position, Vector2Int size, CorridorRoom.EOrientation orientation)
    {
        if (corridorPrefab == null) return;

        Vector3 center = position * roomScale;

        CorridorRoom corridor = Instantiate(corridorPrefab, center, Quaternion.identity, transform);
        corridor.InitializeCorridor(orientation, size);
        corridor.StartGenerating();
    }

    private BoundsInt FindClosestPointTo(BoundsInt currentRoom, List<BoundsInt> rooms)
    {
        BoundsInt closest = new BoundsInt();
        float distance = float.MaxValue;

        Vector3 currentCenter = currentRoom.center;

        foreach (var room in rooms)
        {
            Vector3 otherCenter = room.center;

            float currentDistance = Vector2.Distance(new Vector2(currentCenter.x, currentCenter.z), new Vector2(otherCenter.x, otherCenter.z));

            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = room;
            }
        }

        return closest;
    }

    private void SpawnRoom(BoundsInt roomBounds)
    {
        Vector3 roomCenter = roomBounds.center;
        Vector3 worldCenter = new Vector3(roomCenter.x * roomScale, 0f, roomCenter.z * roomScale);
        Vector2 roomSize = new Vector2(roomBounds.size.x * roomScale, roomBounds.size.z * roomScale);

        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            Debug.LogError("No room prefabs assigned in the generator!");
            return;
        }

        DungeonRoom randomRoomPrefab = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Length)];
        DungeonRoom newRoom = Instantiate(randomRoomPrefab, worldCenter, Quaternion.identity, transform);
        newRoom.StartGenerating();
    }
}
