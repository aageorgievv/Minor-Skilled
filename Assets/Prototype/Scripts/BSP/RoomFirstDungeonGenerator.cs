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
    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        roomBoundsMap.Clear();

        int padding = 2;
        int minPartitionX = minXWidth + (padding * 2);
        int minPartitionZ = minZWidth + (padding * 2);

        BoundsInt dungeonBounds = new BoundsInt(startPosition, new Vector3Int(dungeonSizeX, 1, dungeonSizeZ));

        var roomsList = BinarySpacePartitioningAlgorithm.BinarySpacePartitioning(dungeonBounds, minPartitionX, minPartitionZ);

        List<Vector2Int> roomCenters = new List<Vector2Int>();

        foreach (var roomBounds in roomsList)
        {
            Vector3Int newMin = new Vector3Int(
                roomBounds.min.x + padding,
                roomBounds.min.y,
                roomBounds.min.z + padding);

            Vector3Int newSize = new Vector3Int(
                roomBounds.size.x - (padding * 2),
                roomBounds.size.y,
                roomBounds.size.z - (padding * 2));

            if (newSize.x <= 0 || newSize.z <= 0)
            {
                continue;
            }

            BoundsInt newBounds = new BoundsInt(newMin, newSize);
            Vector3Int center3D = Vector3Int.RoundToInt(newBounds.center);
            Vector2Int center2D = new Vector2Int(center3D.x, center3D.z);

            SpawnRoom(newBounds);

            roomCenters.Add(center2D);
            roomBoundsMap.Add(center2D, newBounds);
        }

        //ConnectRooms(roomCenters);
    }

    private void ConnectRooms(List<Vector2Int> roomCenters)
    {
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            SpawnCorridorConnection(currentRoomCenter, closest);
            currentRoomCenter = closest;
        }
    }

    private void SpawnCorridorConnection(Vector2Int startPos, Vector2Int endPos)
    {
        // Start over 
    }

    private void SpawnCorridor(BoundsInt bounds, CorridorRoom.EOrientation orientation)
    {
        if (corridorPrefab == null) return;

        Vector3 center = bounds.center * roomScale;
        center = new Vector3(Mathf.Floor(center.x), 0, Mathf.Floor(center.z));

        CorridorRoom corridor = Instantiate(corridorPrefab, center, Quaternion.identity, transform);
        corridor.InitializeCorridor(orientation, new Vector2Int(bounds.size.x, bounds.size.z));
        corridor.StartGenerating();
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;

        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);

            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }

        return closest;
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
        newRoom.StartGenerating();
    }
}
