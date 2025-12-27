using System;
using System.Collections.Generic;
using UnityEngine;
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

    private Dictionary<Vector2Int, BoundsFloat> roomBoundsMap = new Dictionary<Vector2Int, BoundsFloat>();

    private const int padding = 2;
    private const int lenghtOffset = 2;

    private readonly List<GridRoom> rooms = new List<GridRoom>();
    private int[,] gridCellIds;

    private const int walkableId = 0;
    private const int topLeftCornerId = 1;
    private const int bottomLeftCornerId = 2;
    private const int topRightCornerId = 3;
    private const int bottomRightCornerId = 4;
    private const int northWallId = 5;
    private const int southWallId = 6;
    private const int eastWallId = 7;
    private const int westWallId = 8;

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        gridCellIds = new int[dungeonSizeX, dungeonSizeZ];

        BoundsInt dungeonBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(dungeonSizeX, 0, dungeonSizeZ));
        List<BoundsInt> roomsList = BinarySpacePartitioningAlgorithm.BinarySpacePartitioning(dungeonBounds, minXWidth, minZWidth);

        rooms.Clear();
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
            // put north and south walls
            for (int x = 0; x < room.Width; x++)
            {
                int roomX = room.X + x;
                int roomMinZ = room.Z;
                int roomMaxZ = room.Z + room.Height - 1;

                if (x == 0) // bottom-left corner
                {
                    gridCellIds[roomX, roomMinZ] = topLeftCornerId;
                    gridCellIds[roomX, roomMaxZ] = bottomLeftCornerId;
                }
                else if (x == room.Width - 1) // bottom-right corner
                {
                    gridCellIds[roomX, roomMinZ] = topRightCornerId;
                    gridCellIds[roomX, roomMaxZ] = bottomRightCornerId;
                }
                else
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

            // World-space center
            Vector3 center = new Vector3(b.center.x, 0f, b.center.z);

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
        if (rooms == null || rooms.Count == 0)
        {
            return;
        }

        List<DungeonRoom> connectedRooms = new List<DungeonRoom>();
        DungeonRoom start = rooms[0];
        connectedRooms.Add(start);

        while (connectedRooms.Count < rooms.Count)
        {
            DungeonRoom bestFrom = null;
            DungeonRoom bestTo = null;
            float bestDistance = float.MaxValue;

            foreach (DungeonRoom from in connectedRooms)
            {
                foreach (DungeonRoom to in rooms)
                {
                    if (connectedRooms.Contains(to) || ReferenceEquals(from, to))
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(from.Bounds.center, to.Bounds.center);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestFrom = from;
                        bestTo = to;
                    }
                }
            }

            if (bestFrom != null && bestTo != null)
            {
                SpawnCorridorConnection(bestFrom, bestTo);
                connectedRooms.Add(bestTo);
            }
            else
            {
                break;
            }
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
            int startX;
            int endX;

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
            int corridorZ = Mathf.RoundToInt(Mathf.Lerp(a.center.z, b.center.z, 0.5f));

            if (lengthX <= 0) return;

            float middlePointX = (startX + endX) / 2f;
            Vector3 position = new Vector3(middlePointX, 0f, corridorZ);

            SpawnCorridor(position, new Vector2Int(lengthX - lenghtOffset, corridorWidth), CorridorRoom.EOrientation.Horizontal);
        }
        else
        {
            // Vertical
            int startZ;
            int endZ;

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
            int corridorX = Mathf.RoundToInt(Mathf.Lerp(a.center.x, b.center.x, 0.5f));

            if (lengthZ <= 0) return;

            float middlePointZ = (startZ + endZ) / 2f;
            Vector3 position = new Vector3(corridorX, 0f, middlePointZ);

            SpawnCorridor(position, new Vector2Int(corridorWidth, lengthZ - lenghtOffset), CorridorRoom.EOrientation.Vertical);
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
