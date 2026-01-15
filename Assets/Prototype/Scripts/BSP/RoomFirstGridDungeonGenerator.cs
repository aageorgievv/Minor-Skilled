using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;


public class RoomFirstGridDungeonGenerator : AbstractDungeonGenerator
{
    [SerializeField]
    private float cellSize;

    [Header("Room Settings")]
    [SerializeField] private int minXWidth;
    [SerializeField] private int minZWidth;

    [Header("Dungeon Settings")]
    [SerializeField] private int dungeonSizeX;
    [SerializeField] private int dungeonSizeZ;

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

        while (true)
        {
            //if (rooms.Count == 0)
            //{
            //    Debug.LogError($"No rooms found to split");
            //    break;
            //}
            //GridRoom roomToSplit2 = rooms.Dequeue();

            if (!rooms.TryDequeue(out GridRoom roomToSplit))
            {
                Debug.LogError($"No rooms found to split");
                break;
            }

            if (!BinarySpacePartitioningAlgorithm.SplitRoom(roomToSplit, minRoomX, minRoomZ, out GridRoom roomA, out GridRoom roomB))
            {
                // cannot split anymore
                break;
            }

            // add walls to the rooms
            AddWalls(roomA);
            AddWalls(roomB);
            ConnectRooms(roomA, roomB);

            rooms.Enqueue(roomA);
            rooms.Enqueue(roomB);
        }
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
                gridCellIds[roomX, roomMinZ] = topLeftCornerId;
                gridCellIds[roomX, roomMaxZ] = bottomLeftCornerId;
            }
            else if (x == room.Width - 1) // bottom and top right corners
            {
                gridCellIds[roomX, roomMinZ] = topRightCornerId;
                gridCellIds[roomX, roomMaxZ] = bottomRightCornerId;
            }
            else // put north and south walls
            {
                gridCellIds[roomX, roomMinZ] = northWallId;
                gridCellIds[roomX, roomMaxZ] = southWallId;
            }
        }

        for (int z = 0; z < room.Height; z++)
        {
            // Corners are already done
            if (z == 0)
            {
                continue;
            }

            // Corners are already done
            if (z == room.Height - 1)
            {
                continue;
            }

            int minX = room.X;
            int maxX = room.X + room.Width - 1;
            int roomZ = room.Z + z;

            gridCellIds[minX, roomZ] = eastWallId;
            gridCellIds[maxX, roomZ] = westWallId;
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
        int roomAX = roomA.X + roomA.Width - 1;
        int roomAZ = roomA.Z + (roomA.Height / 2);

        gridCellIds[roomAX, roomAZ - 1] = bottomLeftCornerId;
        gridCellIds[roomAX, roomAZ] = walkableId;
        gridCellIds[roomAX, roomAZ + 1] = walkableId;
        gridCellIds[roomAX, roomAZ + 2] = topLeftCornerId;

        int roomBX = roomB.X;
        int roomBZ = roomB.Z + (roomB.Height / 2);

        gridCellIds[roomBX, roomBZ - 1] = bottomRightCornerId;
        gridCellIds[roomBX, roomBZ] = walkableId;
        gridCellIds[roomBX, roomBZ + 1] = walkableId;
        gridCellIds[roomBX, roomBZ + 2] = topRightCornerId;
        Debug.Log("H");
    }


    private void ConnectVertically(GridRoom roomA, GridRoom roomB)
    {
        int roomAX = roomA.X + (roomA.Width / 2);
        int roomAZ = roomA.Z + roomA.Height - 1;

        gridCellIds[roomAX - 1, roomAZ] = topRightCornerId;
        gridCellIds[roomAX, roomAZ] = walkableId;
        gridCellIds[roomAX + 1, roomAZ] = walkableId;
        gridCellIds[roomAX + 2, roomAZ] = topLeftCornerId;

        int roomBX = roomB.X + (roomB.Width / 2);
        int roomBZ = roomB.Z;

        gridCellIds[roomBX - 1, roomBZ] = bottomRightCornerId;
        gridCellIds[roomBX, roomBZ] = walkableId;
        gridCellIds[roomBX + 1, roomBZ] = walkableId;
        gridCellIds[roomBX + 2, roomBZ] = bottomLeftCornerId;
        Debug.Log("V");
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

                if (!idToPrefabKVP.TryGetValue(id, out GameObject prefab) && id != 0)
                {
                    Debug.LogError($"Prefab doesn't exist for the given ID: {id}");
                    continue;
                }

                GameObject spawnedObject = null;
                switch (id)
                {
                    //Spawn prefabs based on ID
                    case walkableId:
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
