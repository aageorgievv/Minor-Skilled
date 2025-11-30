using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;
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

    private Dictionary<Vector2Int, BoundsFloat> roomBoundsMap = new Dictionary<Vector2Int, BoundsFloat>();

    private float roomScale = 4f;
    const int padding = 2;

    GameObject cube = null;

    protected override void RunProceduralGeneration()
    {
        if (cube == null)
        {
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "DEBUGCUBE";
            cube.GetComponent<Renderer>().material.color = Color.red;
        }
        CreateRooms();
    }

    private void CreateRooms()
    {
        roomBoundsMap.Clear();

        int minPartitionX = minXWidth + (padding * 2);
        int minPartitionZ = minZWidth + (padding * 2);

        BoundsInt dungeonBounds = new BoundsInt(startPosition, new Vector3Int(dungeonSizeX, 1, dungeonSizeZ));

        var roomsList = BinarySpacePartitioningAlgorithm.BinarySpacePartitioning(dungeonBounds, minPartitionX, minPartitionZ);

        List<DungeonRoom> rooms = new List<DungeonRoom>();

        foreach (var roomBounds in roomsList)
        {
            //Vector3Int newMin = new Vector3Int(
            //    roomBounds.min.x + padding * 2,
            //    roomBounds.min.y,
            //    roomBounds.min.z + padding * 2);

            //Vector3Int newSize = new Vector3Int(
            //    roomBounds.size.x - (padding * 2),
            //    roomBounds.size.y,
            //    roomBounds.size.z - padding * 2);

            //if (newSize.x <= 0 || newSize.z <= 0)
            //{
            //    continue;
            //}

            //BoundsInt newBounds = new BoundsInt(newMin, newSize);
            //Vector3Int center3D = Vector3Int.RoundToInt(newBounds.center);
            //Vector2Int center2D = new Vector2Int(center3D.x, center3D.z);

            var room = SpawnRoom(roomBounds);
            rooms.Add(room);

            //roomBoundsMap.Add(center2D, newBounds);
        }

        ConnectRooms(rooms);
    }

    private void ConnectRooms(List<DungeonRoom> rooms)
    {
        var currentRoomCenter = rooms[Random.Range(0, rooms.Count)];
        rooms.Remove(currentRoomCenter);

        while (rooms.Count > 0)
        {
            DungeonRoom closest = FindClosestPointTo(currentRoomCenter, rooms);
            rooms.Remove(closest);
            //DrawDebugBox(currentRoomCenter, closest);
            SpawnCorridorConnection(currentRoomCenter, closest);
            currentRoomCenter = closest;
        }
    }

    void DrawDebugBox(BoundsInt startRoom, BoundsInt endRoom)
    {
        BoundsInt leftRoom = (startRoom.x < endRoom.x) ? startRoom : endRoom;
        BoundsInt rightRoom = (startRoom.x > endRoom.x) ? startRoom : endRoom;

        BoundsInt roomBounds = leftRoom;
        cube.transform.position = new Vector3(roomBounds.center.x * roomScale, 0f, roomBounds.center.z * roomScale) - new Vector3(8, 0, 0);


        //Vector3 pos = new Vector3(leftRoom.x, leftRoom.position.y, leftRoom.position.z);
        //cube.transform.position = pos * roomScale;
    }

    private void SpawnCorridorConnection(DungeonRoom startRoom, DungeonRoom endRoom)
    {
        //Horizontal
        if (startRoom.Bounds.x != endRoom.Bounds.x)
        {

            float corridorStartX;
            float corridorEndX;

            if (startRoom.Bounds.x < endRoom.Bounds.x)
            {
                corridorStartX = startRoom.Bounds.center.x + startRoom.size.x / 2f;
                corridorEndX = endRoom.Bounds.center.x - endRoom.size.x / 2;
            }
            else
            {
                corridorStartX = endRoom.Bounds.center.x + endRoom.size.x / 2f; // accounting for the walls
                corridorEndX = startRoom.Bounds.center.x - startRoom.size.x / 2f;
            }

            float x = (corridorEndX + corridorStartX) / 2f;
            float lengthX = Mathf.Abs(corridorEndX - corridorStartX) + padding * 2;

            int corridorZ = Mathf.RoundToInt(startRoom.Bounds.center.z);
            Vector3 position = new Vector3(x, 0, corridorZ - corridorWidth / 2);

            SpawnCorridor(position, new Vector2Int((int)lengthX, corridorWidth), CorridorRoom.EOrientation.Horizontal);
        }
        else if (startRoom.Bounds.z != endRoom.Bounds.z)
        {
            float corridorStartZ;
            float corridorEndZ;

            if (startRoom.Bounds.z < endRoom.Bounds.z)
            {
                corridorStartZ = startRoom.Bounds.center.z + startRoom.size.y / 2f;
                corridorEndZ = endRoom.Bounds.center.z - endRoom.size.y / 2;
            }
            else
            {
                corridorStartZ = endRoom.Bounds.center.z + endRoom.size.y / 2f; // accounting for the walls
                corridorEndZ = startRoom.Bounds.center.z - startRoom.size.y / 2f;
            }

            float z = (corridorEndZ + corridorStartZ) / 2f;
            float lengthZ = Mathf.Abs(corridorEndZ - corridorStartZ) + padding * 2;

            int corridorZ = Mathf.RoundToInt(startRoom.Bounds.center.z);
            Vector3 position = new Vector3(corridorZ - corridorWidth / 2, 0f, z);

            SpawnCorridor(position, new Vector2Int(corridorWidth, (int)lengthZ), CorridorRoom.EOrientation.Vertical);
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

    private DungeonRoom FindClosestPointTo(DungeonRoom currentRoom, List<DungeonRoom> rooms)
    {
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
        Vector3 worldCenter = new Vector3(roomCenter.x * roomScale, 0f, roomCenter.z * roomScale);

        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            Debug.LogError("No room prefabs assigned in the generator!");
            return null;
        }


        DungeonRoom randomRoomPrefab = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Length)];
        DungeonRoom newRoom = Instantiate(randomRoomPrefab, worldCenter, Quaternion.identity, transform);
        newRoom.Bounds = roomBounds;
        newRoom.StartGenerating();
        return newRoom;
    }
}
