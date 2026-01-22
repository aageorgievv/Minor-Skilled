using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using Color = UnityEngine.Color;


public class RoomFirstGridDungeonGenerator : AbstractDungeonGenerator
{
    [SerializeField] GameObject prefab;

    [Header("Room Settings")]
    [SerializeField] private int minXWidth;
    [SerializeField] private int minZWidth;

    [Header("Dungeon Settings")]
    [SerializeField] private int dungeonSizeX;
    [SerializeField] private int dungeonSizeZ;
    [SerializeField] private float cellSize;

    [Header("Corridor Settings")]
    [SerializeField] private int corridorWidth = 2;
    [SerializeField] private CorridorRoom corridorPrefab;

    private Dictionary<int, GameObject> idToPrefabKVP = new Dictionary<int, GameObject>();

    //For Clearing purposes
    [SerializeField] private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private int[,] gridCellIds;

    private const int walkableId = 0;
    private const int topLeftCornerId = 1;
    private const int topRightCornerId = 2;
    private const int bottomLeftCornerId = 3;
    private const int bottomRightCornerId = 4;
    private const int northWallId = 5;
    private const int southWallId = 6;
    private const int eastWallId = 7;
    private const int westWallId = 8;
    private const int corridorWalkableId = 9;


    private const int corridorSideOffset = 6;

    protected override void RunProceduralGeneration()
    {
        DeleteProceduralGeneration();

        idToPrefabKVP.Clear();
        idToPrefabKVP.Add(northWallId, wallPrefab);
        idToPrefabKVP.Add(southWallId, wallPrefab);
        idToPrefabKVP.Add(westWallId, wallPrefab);
        idToPrefabKVP.Add(eastWallId, wallPrefab);
        idToPrefabKVP.Add(topLeftCornerId, cornerPrefab);
        idToPrefabKVP.Add(topRightCornerId, cornerPrefab);
        idToPrefabKVP.Add(bottomLeftCornerId, cornerPrefab);
        idToPrefabKVP.Add(bottomRightCornerId, cornerPrefab);

        GenerateDungeon(dungeonSizeX, dungeonSizeZ, minXWidth, minZWidth);

        SpawnWalls();
    }
    protected override void DeleteProceduralGeneration()
    {
        gridCellIds = null;

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            GameObject spawnedObject = spawnedObjects[i];
            DestroyImmediate(spawnedObject);
        }

        spawnedObjects.Clear();
    }

    public void GenerateDungeon(int dungeonWidth, int dungeonHeight, int minRoomX, int minRoomZ)
    {
        gridCellIds = new int[dungeonWidth, dungeonHeight];

        Queue<GridRoom> rooms = new Queue<GridRoom>();

        GridRoom dungeon = new GridRoom(0, 0, dungeonWidth, dungeonHeight);
        rooms.Enqueue(dungeon);

        List<GridRoom> dungeonRooms = new List<GridRoom>();

        while (true)
        {
            if (!rooms.TryDequeue(out GridRoom roomToSplit))
            {
                //Debug.LogError($"No rooms found to split");
                break;
            }

            if (!BinarySpacePartitioningAlgorithm.SplitRoom(roomToSplit, minRoomX, minRoomZ, out GridRoom roomA, out GridRoom roomB))
            {
                dungeonRooms.Add(roomToSplit);
                // cannot split anymore
                continue;
            }

            // add walls to the rooms
            AddWalls(roomA);
            AddWalls(roomB);
            ConnectRooms(roomA, roomB);

            rooms.Enqueue(roomA);
            rooms.Enqueue(roomB);
        }

        GenerateFurniture(dungeonRooms);
    }

    private void GenerateFurniture(List<GridRoom> dungeonRooms)
    {
        foreach (GridRoom room in dungeonRooms)
        {
            var sections = GetSections(room, 2, 2);

            foreach (var section in sections)
            {
                SpawnFurniture(section);
            }
        }
    }

    private List<GridRoom> GetSections(GridRoom room, int minW, int minH)
    {
        List<GridRoom> sections = new List<GridRoom>();

        if (!BinarySpacePartitioningAlgorithm.SplitRoom(room, minW, minH, out var roomA, out var roomB))
        {
            sections.Add(room);
            return sections;
        }

        sections.AddRange(GetSections(roomA, minW, minH));
        sections.AddRange(GetSections(roomB, minW, minH));

        return sections;
    }


    private void SpawnFurniture(GridRoom section)
    {
        Vector3 position = Grid.ToWorldPositionCenter(section.X, section.Z, cellSize);

        GameObject gameObject = Instantiate(prefab, position, Quaternion.identity, transform);
        spawnedObjects.Add(gameObject);
    }


    private void AddWalls(GridRoom room)
    {
        for (int x = 0; x < room.Width; x++)
        {
            int roomX = room.X + x;
            int roomMinZ = room.Z;
            int roomMaxZ = room.Z + room.Height - 1;

            if (x == 0) // bottom and top left corners
            {

                SetIfNotCorner(roomX, roomMinZ, topLeftCornerId);
                SetIfNotCorner(roomX, roomMaxZ, bottomLeftCornerId);
            }
            else if (x == room.Width - 1) // bottom and top right corners
            {
                SetIfNotCorner(roomX, roomMinZ, topRightCornerId);
                SetIfNotCorner(roomX, roomMaxZ, bottomRightCornerId);
            }
            else // put north and south walls
            {
                SetIfNotCorner(roomX, roomMinZ, northWallId);
                SetIfNotCorner(roomX, roomMaxZ, southWallId);
            }
        }

        for (int z = 0; z < room.Height; z++)
        {
            int minX = room.X;
            int maxX = room.X + room.Width - 1;
            int roomZ = room.Z + z;

            SetIfNotCorner(minX, roomZ, eastWallId);
            SetIfNotCorner(maxX, roomZ, westWallId);
        }
    }

    private void ConnectRooms(GridRoom roomA, GridRoom roomB)
    {
        if (roomA.X != roomB.X)
        {
            if (roomA.X < roomB.X)
            {
                ConnectHorizontally(roomA, roomB);
            }
            else
            {
                ConnectHorizontally(roomB, roomA);
            }
        }
        else
        {
            if (roomA.Z < roomB.Z)
            {
                ConnectVertically(roomA, roomB);
            }
            else
            {
                ConnectVertically(roomB, roomA);
            }
        }
    }

    private void ConnectHorizontally(GridRoom roomA, GridRoom roomB)
    {
        bool leftOrRight = Random.value > 0.5f;

        int roomAX = roomA.X + roomA.Width - 1;
        int roomAZ = leftOrRight ? roomA.Z + roomA.Height - corridorSideOffset : roomA.Z + corridorSideOffset;

        SetRoom(roomAX, roomAZ - 1, bottomLeftCornerId);
        SetRoom(roomAX, roomAZ, corridorWalkableId);
        SetRoom(roomAX, roomAZ + 1, corridorWalkableId);
        SetRoom(roomAX, roomAZ + 2, topLeftCornerId);

        int roomBX = roomB.X;
        int roomBZ = leftOrRight ? roomB.Z + roomB.Height - corridorSideOffset : roomB.Z + corridorSideOffset;

        SetRoom(roomBX, roomBZ - 1, bottomRightCornerId);
        SetRoom(roomBX, roomBZ, corridorWalkableId);
        SetRoom(roomBX, roomBZ + 1, corridorWalkableId);
        SetRoom(roomBX, roomBZ + 2, topRightCornerId);
    }


    private void ConnectVertically(GridRoom roomA, GridRoom roomB)
    {
        bool leftOrRight = Random.value > 0.5f;

        int roomAX = leftOrRight ? roomA.X + roomA.Width - corridorSideOffset : roomA.X + corridorSideOffset;
        int roomAZ = roomA.Z + roomA.Height - 1;

        SetRoom(roomAX - 1, roomAZ, topRightCornerId);
        SetRoom(roomAX, roomAZ, corridorWalkableId);
        SetRoom(roomAX + 1, roomAZ, corridorWalkableId);
        SetRoom(roomAX + 2, roomAZ, topLeftCornerId);

        int roomBX = leftOrRight ? roomB.X + roomB.Width - corridorSideOffset : roomB.X + corridorSideOffset;
        int roomBZ = roomB.Z;

        SetRoom(roomBX - 1, roomBZ, bottomRightCornerId);
        SetRoom(roomBX, roomBZ, corridorWalkableId);
        SetRoom(roomBX + 1, roomBZ, corridorWalkableId);
        SetRoom(roomBX + 2, roomBZ, bottomLeftCornerId);
    }

    private void SetRoom(int x, int y, int roomId)
    {
        if (x < 0 || x >= gridCellIds.GetLength(0) || y < 0 ||  y >= gridCellIds.GetLength(1))
        {
            return;
        }

        gridCellIds[x, y] = roomId;
    }

    private void SpawnWalls()
    {
        spawnedObjects.Clear();

        int xCells = gridCellIds.GetLength(0);
        int zCells = gridCellIds.GetLength(1);

        for (int z = 0; z < zCells; z++)
        {
            for (int x = 0; x < xCells; x++)
            {
                int id = gridCellIds[x, z];

                if (!idToPrefabKVP.TryGetValue(id, out GameObject prefab) && id != 0 && id != 9)
                {
                    Debug.LogError($"Prefab doesn't exist for the given ID: {id}");
                    continue;
                }

                GameObject spawnedObject = null;
                switch (id)
                {
                    //Spawn prefabs based on ID
                    case walkableId:
                    case corridorWalkableId:
                        // Do nothing
                        break;
                    case topLeftCornerId:
                        spawnedObject = Instantiate(prefab, Grid.ToWorldPosition(x, z, cellSize), Quaternion.identity, transform);
                        break;
                    case topRightCornerId:
                        spawnedObject = Instantiate(prefab, Grid.ToWorldPosition(x, z, cellSize), Quaternion.Euler(0, -90, 0), transform);
                        break;
                    case bottomLeftCornerId:
                        spawnedObject = Instantiate(prefab, Grid.ToWorldPosition(x, z, cellSize), Quaternion.Euler(0, 90, 0), transform);
                        break;
                    case bottomRightCornerId:
                        spawnedObject = Instantiate(prefab, Grid.ToWorldPosition(x, z, cellSize), Quaternion.Euler(0, -180, 0), transform);
                        break;
                    case northWallId:
                        spawnedObject = Instantiate(prefab, Grid.ToWorldPosition(x, z, cellSize), Quaternion.identity, transform);
                        break;
                    case southWallId:
                        spawnedObject = Instantiate(prefab, Grid.ToWorldPosition(x, z, cellSize), Quaternion.identity, transform);
                        break;
                    case eastWallId:
                        spawnedObject = Instantiate(prefab, Grid.ToWorldPosition(x, z, cellSize), Quaternion.Euler(0, 90, 0), transform);
                        break;
                    case westWallId:
                        spawnedObject = Instantiate(prefab, Grid.ToWorldPosition(x, z, cellSize), Quaternion.Euler(0, 90, 0), transform);
                        break;
                    default:
                        break;
                }

                if (spawnedObject == null)
                {
                    continue;
                }

                spawnedObjects.Add(spawnedObject);
            }
        }
    }



    private bool IsCornerId(int id)
    {
        return id == topLeftCornerId
            || id == topRightCornerId
            || id == bottomLeftCornerId
            || id == bottomRightCornerId
            || id == corridorWalkableId;
    }

    private void SetIfNotCorner(int x, int z, int newId)
    {
        int existing = gridCellIds[x, z];
        if (IsCornerId(existing)) return;   // keep the corner
        gridCellIds[x, z] = newId;
    }

    private void OnDrawGizmos()
    {
        if (gridCellIds == null)
        {
            return;
        }

        int xCells = gridCellIds.GetLength(0);
        int zCells = gridCellIds.GetLength(1);

        int halfXCells = Mathf.FloorToInt(xCells / 2f);
        int halfZCells = Mathf.FloorToInt(zCells / 2f);

        Color prevColor = Gizmos.color;

        for (int z = 0; z < zCells; z++)
        {
            for (int x = 0; x < xCells; x++)
            {
                int id = gridCellIds[x, z];
                Vector3 position = Grid.ToWorldPosition(x, z, cellSize);

                Color color;
                switch (id)
                {
                    case walkableId:
                    case corridorWalkableId:
                        color = Color.white;
                        break;
                    case topLeftCornerId:
                    case bottomLeftCornerId:
                    case topRightCornerId:
                    case bottomRightCornerId:
                        color = Color.cyan;
                        break;
                    case northWallId:
                    case southWallId:
                    case eastWallId:
                    case westWallId:
                        color = Color.red;
                        break;
                    default:
                        color = Color.magenta;
                        break;

                }

                Gizmos.color = color;
                Gizmos.DrawCube(position, new Vector3(0.9f * cellSize, 0.1f, 0.9f * cellSize));
            }
        }
        Gizmos.color = prevColor;
    }
}
