using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using Random = UnityEngine.Random;


public class RoomFirstDungeonGenerator : AbstractDungeonGenerator
{
    [Header("Debug")]
    [SerializeField] private bool debugSpawnBspPartitions = true;

    [Header("Settings")]
    [SerializeField] private int minXWidth;
    [SerializeField] private int minZWidth;
    [SerializeField] private int dungeonSizeX;
    [SerializeField] private int dungeonSizeZ;

    [Header("Corridor Settings")]
    [SerializeField] private int corridorWidth = 2;
    [SerializeField] private CorridorRoom corridorPrefab;

    private Dictionary<Vector2Int, BoundsFloat> roomBoundsMap = new Dictionary<Vector2Int, BoundsFloat>();

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

        if (debugSpawnBspPartitions)
            DebugSpawnBspPartitions(roomsList);

        List<DungeonRoom> rooms = new List<DungeonRoom>();

        foreach (var roomBounds in roomsList)
        {
            var room = SpawnRoom(roomBounds);
            rooms.Add(room);
        }

        ConnectRooms(rooms);
    }

    private void DebugSpawnBspPartitions(List<BoundsInt> partitions)
    {
        GameObject parent = GameObject.Find("BSP_Debug") ?? new GameObject("BSP_Debug");

        // Clean previous debug meshes
        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < partitions.Count; i++)
        {
            BoundsInt b = partitions[i];

            // World-space center and size
            Vector3 center = new Vector3(
                b.center.x,
                0f,
                b.center.z
            );

            Vector3 size = new Vector3(b.size.x, 0.1f, b.size.z);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(parent.transform);
            cube.transform.position = center;
            cube.transform.localScale = size;

            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(renderer.sharedMaterial);
                renderer.material.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f);
            }
        }
    }

    private void ConnectRooms(List<DungeonRoom> rooms)
    {
        var currentRoomCenter = rooms[Random.Range(0, rooms.Count)];
        rooms.Remove(currentRoomCenter);

        while (rooms.Count > 0)
        {
            DungeonRoom closest = FindClosestPointTo(currentRoomCenter, rooms);
            rooms.Remove(closest);
            SpawnCorridorConnection(currentRoomCenter, closest);
            currentRoomCenter = closest;
        }
    }

    private void SpawnCorridorConnection(DungeonRoom startRoom, DungeonRoom endRoom)
    {
        var a = startRoom.Bounds;
        var b = endRoom.Bounds;

        // Decide whether to connect horizontally or vertically based on which separation is bigger
        bool connectHorizontally = Mathf.Abs(a.center.x - b.center.x) > Mathf.Abs(a.center.z - b.center.z);

        if (connectHorizontally)
        {
            // Horizontal
            int corridorZ = Mathf.RoundToInt(Mathf.Lerp(a.center.z, b.center.z, 0.5f));

            int startX, endX;

            if (a.min.x < b.min.x)
            {
                // a is left, b is right
                startX = a.max.x - padding;
                endX = b.min.x + padding;
            }
            else
            {
                // b is left, a is right
                startX = b.max.x - padding;
                endX = a.min.x + padding;
            }

            int lengthX = Mathf.Abs(endX - startX);
            if (lengthX <= 0) return; // safety

            float x = (startX + endX) / 2f;
            Vector3 position = new Vector3(x, 0f, corridorZ);

            SpawnCorridor(position, new Vector2Int(lengthX, corridorWidth), CorridorRoom.EOrientation.Horizontal);
        }
        else
        {
            // Vertical
            int corridorX = Mathf.RoundToInt(Mathf.Lerp(a.center.x, b.center.x, 0.5f));

            int startZ, endZ;

            if (a.min.z < b.min.z)
            {
                // a is bottom, b is top
                startZ = a.max.z - padding;
                endZ = b.min.z + padding;
            }
            else
            {
                // b is bottom, a is top
                startZ = b.max.z - padding;
                endZ = a.min.z + padding;
            }

            int lengthZ = Mathf.Abs(endZ - startZ);
            if (lengthZ <= 0) return; // safety

            float z = (startZ + endZ) / 2f;
            Vector3 position = new Vector3(corridorX, 0f, z);

            SpawnCorridor(position, new Vector2Int(corridorWidth, lengthZ), CorridorRoom.EOrientation.Vertical);
        }
    }


    private void SpawnCorridor(Vector3 position, Vector2Int size, CorridorRoom.EOrientation orientation)
    {
        if (corridorPrefab == null) return;

        Vector3 center = position;

        CorridorRoom corridor = Instantiate(corridorPrefab, center, Quaternion.identity, transform);
        corridor.InitializeCorridor(orientation, size);
        corridor.StartGenerating();
    }

    private DungeonRoom FindClosestPointTo(DungeonRoom currentRoom, List<DungeonRoom> rooms)
    {
        // TODO: find a better way to find closest room than just compariong centers - check closest corners or something


        DungeonRoom closest = null;
        float distance = float.MaxValue;

        Vector3 currentCenter = currentRoom.Bounds.center;

        foreach (var room in rooms)
        {
            Vector3 otherCenter = room.Bounds.center;

            float currentDistance = Vector2.Distance(new Vector2(currentCenter.x, currentCenter.z), new Vector2(otherCenter.x, otherCenter.z));

            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = room;
            }
        }

        return closest;
    }

    private DungeonRoom SpawnRoom(BoundsInt roomBounds)
    {
        Vector3 roomCenter = roomBounds.center;
        Vector3 worldCenter = new Vector3(roomCenter.x, 0f, roomCenter.z);

        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            Debug.LogError("No room prefabs assigned in the generator!");
            return null;
        }

        DungeonRoom randomRoomPrefab = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Length)];
        DungeonRoom newRoom = Instantiate(randomRoomPrefab, worldCenter, Quaternion.identity, transform);

        newRoom.Bounds = roomBounds;

        int gridWidth = Mathf.Max(1, roomBounds.size.x - padding * 2);
        int gridLength = Mathf.Max(1, roomBounds.size.z - padding * 2);

        newRoom.size = new Vector2Int(gridWidth, gridLength);

        newRoom.StartGenerating();
        return newRoom;
    }

}
