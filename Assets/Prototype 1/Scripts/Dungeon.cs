using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorPrefab;

    [Header("Settings")]
    [SerializeField] public Vector2 size;

    protected List<Room> rooms = new List<Room>();
    protected List<Door> doors = new List<Door>();

    private void Start()
    {
        //Execute this everyTime you resize it at runtime.
        StartGenerating(5);
    }

    private void StartGenerating(int minRoomSize)
    {
        rooms.Clear();
        doors.Clear();

        GenerateRooms();
    }

    private void GenerateRooms()
    {
        foreach (Room room in rooms)
        {
            GenerateWalls();
        }

        foreach (Door door in doors)
        {
            Instantiate(doorPrefab, door.position, Quaternion.identity, transform);
        }
    }

    private void GenerateWalls()
    {
        //Step 1 Generate the Room
        //When resizing the rooms I probably will need to play with the scaling as I increase the X or Z.
        // Make a threshhold as I increase the scale, probably 1.5 max threshhold. I stretch until I exceed that amount and then spawn another wall and resize them to 0.75 or something like that?
    }
}
