using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Dungeon : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject floorPrefab;

    [Header("Interior")]
    [SerializeField] private GameObject longTablePrefab;
    [SerializeField] private GameObject chairPrefab;
    [SerializeField] private GameObject emptyChestPrefab;
    [SerializeField] private GameObject treasureChestPrefab;

    [Header("Settings")]
    [SerializeField, Min(1f)] public Vector2 size;
    [SerializeField] private int tableSizeSpawn = 6;
    [SerializeField] private int chestSizeSpawn = 3;
    [SerializeField] private int maxChestSpawnAmount = 3;


    protected List<Room> rooms = new List<Room>();
    protected List<Door> doors = new List<Door>();

    private Vector2 lastSize;

    private void Update()
    {
        if (size != lastSize)
        {
            lastSize = size;

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            StartGenerating();
        }
    }

    private void StartGenerating()
    {
        rooms.Clear();
        doors.Clear();

        Room dungeonRoom = new Room(Vector3.zero, size.x, size.y);
        rooms.Add(dungeonRoom);

        GenerateRooms();
    }

    private void GenerateRooms()
    {
        foreach (Room room in rooms)
        {
            GenerateWalls(room);
            GenerateFloor(room);
            GenerateInterior(room);
        }

        foreach (Door door in doors)
        {
            Instantiate(doorPrefab, door.position, Quaternion.identity, transform);
        }
    }

    private void GenerateWalls(Room room)
    {
        float maxStretch = 1.5f;
        float wallSize = 4f;

        float halfWidth = room.width / 2f;
        float halfLength = room.length / 2f;

        //Bottom
        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength),
                         Vector3.right, 0, room.width, maxStretch, wallSize);
        //Top
        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z + halfLength),
                         Vector3.right, 0, room.width, maxStretch, wallSize);
        //Left
        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength),
                         Vector3.forward, 90, room.length, maxStretch, wallSize);
        //Right
        GenerateWallLine(new Vector3(room.center.x + halfWidth, 0, room.center.z - halfLength),
                         Vector3.forward, 90, room.length, maxStretch, wallSize);
    }

    private void GenerateWallLine(Vector3 startPos, Vector3 dir, int angle, float totalLength, float maxStretch, float wallSize)
    {
        float placed = 0f;
        while (placed < totalLength)
        {
            float remaining = totalLength - placed;
            float maxCover = wallSize * maxStretch;
            float cover = Mathf.Min(remaining, maxCover);
            float scale = cover / wallSize;

            GameObject wall = Instantiate(wallPrefab, transform);
            wall.transform.position = startPos + dir * (placed + cover / 2);
            wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            wall.transform.localScale = new Vector3(scale, 1f, 1f);

            placed += cover;
        }
    }

    private void GenerateFloor(Room room)
    {
        GameObject floor = Instantiate(floorPrefab, transform);
        floor.transform.position = room.center;
        floor.transform.localScale = new Vector3(room.width, 0.1f, room.length);
    }

    private void GenerateInterior(Room room)
    {
        GenerateTable(room);
        GenerateChest(room);
    }

    private void GenerateTable(Room room)
    {
        //Table and chairs
        if (room.width >= tableSizeSpawn && room.length >= tableSizeSpawn && longTablePrefab != null && chairPrefab != null)
        {
            GameObject table = Instantiate(longTablePrefab, transform);
            table.transform.position = room.center;

            float chairOffsetX = 1.5f;
            float chairOffsetZ = 1f;
            Vector3[] chairPositions =
            {
                room.center + new Vector3(chairOffsetX, 0, -chairOffsetZ),
                room.center + new Vector3(chairOffsetX, 0, chairOffsetZ),
                room.center + new Vector3(-chairOffsetX, 0, -chairOffsetZ),
                room.center + new Vector3(-chairOffsetX, 0, chairOffsetZ),
            };

            Quaternion[] chairOrientations =
            {
                Quaternion.Euler(0, 180, 0),
                Quaternion.Euler(0, 180, 0),
                Quaternion.Euler(0, 0, 0),
                Quaternion.Euler(0, 0, 0),

            };

            for (int i = 0; i < chairPositions.Length; i++)
            {
                Instantiate(chairPrefab, chairPositions[i], chairOrientations[i], transform);
            }
        }
    }

    private void GenerateChest(Room room)
    {
        //Chest
        if (room.width >= chestSizeSpawn && room.length != chestSizeSpawn && emptyChestPrefab != null && treasureChestPrefab != null)
        {
            float halfWidth = room.width / 2f;
            float halfLength = room.length / 2f;
            float offset = 0.75f;
            int chestSpawnAmount = Random.Range(0, maxChestSpawnAmount + 1);

            for (int i = 0; i < chestSpawnAmount; i++)
            {
                int wall = Random.Range(0, 4);
                Vector3 chestPosition = Vector3.zero;
                Quaternion chestRotation = Quaternion.identity;

                switch (wall)
                {
                    case 0: // Top wall
                        chestPosition = new Vector3(
                            Random.Range(room.center.x - halfWidth + offset, room.center.x + halfWidth - offset), 0, room.center.z + halfLength - offset);
                        chestRotation = Quaternion.Euler(0, 180, 0);
                        break;
                    case 1: // Bottom wall
                        chestPosition = new Vector3(Random.Range(room.center.x - halfWidth + offset, room.center.x + halfWidth - offset), 0, room.center.z - halfLength + offset);
                        chestRotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case 2: // Right wall
                        chestPosition = new Vector3(room.center.x + halfWidth - offset, 0, Random.Range(room.center.z - halfLength + offset, room.center.z + halfLength - offset));
                        chestRotation = Quaternion.Euler(0, -90, 0);
                        break;
                    case 3: // Left wall
                        chestPosition = new Vector3(room.center.x - halfWidth + offset, 0, Random.Range(room.center.z - halfLength + offset, room.center.z + halfLength - offset));
                        chestRotation = Quaternion.Euler(0, 90, 0);
                        break;
                }

                GameObject chestPrefab = Random.value < 0.5f ? emptyChestPrefab : treasureChestPrefab;
                Instantiate(chestPrefab, chestPosition, chestRotation, transform);
            }
        }
    }
}
