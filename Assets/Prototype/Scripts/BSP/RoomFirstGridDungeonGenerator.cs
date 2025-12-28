using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;


public class RoomFirstGridDungeonGenerator : AbstractDungeonGenerator
{
    [SerializeField]
    private float cellSize = 50f;

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

    private const int padding = 2;
    private const int lenghtOffset = 2;

    private readonly List<GridRoom> rooms = new List<GridRoom>();
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
        GenerateRooms();
        SpawnWalls();
    }
    protected override void DeleteProceduralGeneration()
    {
        rooms.Clear();
        gridCellIds = null;

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            GameObject spawnedObject = spawnedObjects[i];
            DestroyImmediate(spawnedObject);
        }

        spawnedObjects.Clear();
    }

    private void GenerateRooms()
    {
        gridCellIds = new int[dungeonSizeX, dungeonSizeZ];
        rooms.Clear();
        idToPrefabKVP.Clear();

        idToPrefabKVP.Add(northWallId, wallPrefab);
        idToPrefabKVP.Add(southWallId, wallPrefab);
        idToPrefabKVP.Add(westWallId, wallPrefab);
        idToPrefabKVP.Add(eastWallId, wallPrefab);
        idToPrefabKVP.Add(topLeftCornerId, cornerPrefab);
        idToPrefabKVP.Add(topRightCornerId, cornerPrefab);
        idToPrefabKVP.Add(bottomLeftCornerId, cornerPrefab);
        idToPrefabKVP.Add(bottomRightCornerId, cornerPrefab);


        BoundsInt dungeonBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(dungeonSizeX, 0, dungeonSizeZ));
        List<BoundsInt> roomsList = BinarySpacePartitioningAlgorithm.BinarySpacePartitioning(dungeonBounds, minXWidth, minZWidth);

        foreach (BoundsInt roomBounds in roomsList)
        {
            int x = roomBounds.min.x;
            int z = roomBounds.min.z;
            int width = roomBounds.size.x;
            int height = roomBounds.size.z;
            GridRoom room = new GridRoom(x, z, width, height);
            rooms.Add(room);
        }

        CreateWalls();
    }

    private void CreateWalls()
    {
        foreach (GridRoom room in rooms)
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
                // we already did corners above
                if (z == 0)
                {
                    continue;
                }

                // we already did corners above
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

                if(!idToPrefabKVP.TryGetValue(id, out GameObject prefab) && id != 0)
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

                if(spawnedObject == null)
                {
                    continue;
                }

                spawnedObjects.Add(spawnedObject);
            }
        }
    }
}
