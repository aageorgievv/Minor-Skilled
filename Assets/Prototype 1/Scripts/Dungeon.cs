using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorPrefab;

    [Header("Settings")]
    [SerializeField, Min(1f)] public Vector2 size;

    protected List<Room> rooms = new List<Room>();
    protected List<Door> doors = new List<Door>();

    private Vector2 lastSize;

    private void Update()
    {
        if (size != lastSize)
        {
            lastSize = size;

            // Clear old dungeon
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            // Rebuild with new size
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

        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength),
                         Vector3.right, 0, room.width, maxStretch, wallSize);

        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z + halfLength),
                         Vector3.right, 0, room.width, maxStretch, wallSize);

        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength),
                         Vector3.forward , 90, room.length, maxStretch, wallSize);

        GenerateWallLine(new Vector3(room.center.x + halfWidth, 0, room.center.z - halfLength),
                         Vector3.forward, 90,  room.length, maxStretch, wallSize);
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
            wall.transform.position = startPos + dir * (placed + cover / 2f);
            wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            wall.transform.localScale = new Vector3(scale, 1f, 1f);

            placed += cover;
        }
    }
}
