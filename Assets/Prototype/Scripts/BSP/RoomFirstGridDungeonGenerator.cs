using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using Color = UnityEngine.Color;

public enum ESpawnLocation
{
    Corner,
    Wall,
    Center
}

public class RoomFirstGridDungeonGenerator : AbstractDungeonGenerator
{

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

    //Furniture Ids
    private const int furnitureBaseId = 10;

    private int nextFurnitureId;

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
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
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
            //avoiding the walls
            GridRoom roomInterior = new GridRoom(room.X + 1, room.Z + 1, room.Width - 2, room.Height - 2);
            SpawnFurniture(roomInterior);
        }
    }


/*    private void SpawnFurniture(GridRoom section)
    {
        // prefab has some script for the sizes
        *//*        int sizeX = 2;
                int sizeZ = 1;
                ESpawnLocation[] spawnLocations = (ESpawnLocation[])Enum.GetValues(typeof(ESpawnLocation));
                int idx = UnityEngine.Random.Range(0, spawnLocations.Length);
                ESpawnLocation wallDirection = spawnLocations[idx];

                for (int z = 0; z < section.Height; z++)
                {
                    for (int x = 0; x < section.Width; x++)
                    {
                        if (HasFurniture(x, z))
                        {
                            continue;
                        }

                        switch (wallDirection)
                        {
                            case ESpawnLocation.NorthWall:
                                if (!IsWall(x, z - 1))
                                {
                                    continue;
                                }

                                break;
                            case ESpawnLocation.EastWall:
                                if (!IsWall(x + 1, z))
                                {
                                    continue;
                                }

                                break;
                            case ESpawnLocation.SouthWall:
                                if (!IsWall(x, z + 1))
                                {
                                    continue;
                                }

                                break;
                            case ESpawnLocation.WestWall:
                                if (!IsWall(x - 1, z))
                                {
                                    continue;
                                }

                                break;
                            case ESpawnLocation.CenterRoom:
                                if (!IsWall(x, z - 1) ||
                                    !IsWall(x + 1, z) ||
                                    !IsWall(x, z + 1) ||
                                    !IsWall(x - 1, z))
                                {
                                    continue;
                                }

                                break;

                        }

                        bool spawnHorizontally = sizeX <= section.Width;
                        bool spawnVertically = sizeZ <= section.Height;

                        if (!spawnHorizontally || !spawnVertically)
                        {
                            // not enough space error
                            return;
                        }

                        int X = UnityEngine.Random.Range(section.X, section.X + section.Width - sizeX);
                        int Z = UnityEngine.Random.Range(section.Z, section.Z + section.Height - sizeZ);

                        Quaternion rotation = Quaternion.identity;
                        if (spawnVertically)
                        {
                            rotation = Quaternion.Euler(0, 90, 0);
                        }


                        //gridCellIds[X, Z] = whateverYouSpawnedId;
                        return;
                    }
                }*//*


        Vector3 position = Grid.ToWorldPositionCenter(section.X, section.Z, cellSize);

        GameObject gameObject = Instantiate(prefab, position, Quaternion.identity, transform);
        spawnedObjects.Add(gameObject);
    }*/

    /*    private bool IsCorner(int x, int z)
        {
            int northCellId = gridCellIds[x, z - 1];
            int eastCellId = gridCellIds[x + 1, z];
            int southCellId = gridCellIds[x, z + 1];
            int westCellId = gridCellIds[x - 1, z];

            // is north-east
            if (northCellId == northWallId &&
                eastCellId == eastWallId)
            {
                return true;
            }

            // south-east
            if (eastCellId == eastWallId &&
                southCellId == southWallId)
            {
                return true;
            }

            // south-west
            if (southCellId == southWallId &&
                westCellId == westWallId)
            {
                return true;
            }

            // north-west
            if (westCellId == westWallId &&
                northCellId == northWallId)
            {
                return true;
            }

            return false;
        }*/


    private void SpawnFurniture(GridRoom section)
    {
        if (furniturePrefabs == null || furniturePrefabs.Length == 0)
        {
            return;
        }

        foreach (var config in furniturePrefabs)
        {
            if (config.prefab == null || config.count <= 0) continue;

            IFurnitureFootPrint footprintComp = config.prefab.GetComponent<IFurnitureFootPrint>();

            if (footprintComp == null)
            {
                Debug.LogError($"{config.prefab.name} missing IFurnitureFootprint");
                continue;
            }

            List<Placement> placementList = CollectPlacements(section, footprintComp);
            HashSet<ESpawnLocation> allowed = new HashSet<ESpawnLocation>(footprintComp.AllowedLocations);
            placementList.RemoveAll(p => !allowed.Contains(p.Type));

            for (int i = 0; i < placementList.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, placementList.Count);
                (placementList[i], placementList[j]) = (placementList[j], placementList[i]);
            }

            int placed = 0;
            foreach (var placement in placementList)
            {
                if (placed >= config.count) break;

                if (CanPlaceFootprint(placement.Cell, footprintComp.FootPrint, placement.Rot))
                {
                    Vector3 world = Grid.ToWorldPositionCenter(placement.Cell.x, placement.Cell.y, cellSize);
                    Instantiate(config.prefab, world, placement.Rot, transform);

                    int id = furnitureBaseId + nextFurnitureId++;
                    MarkFurniture(placement.Cell, footprintComp.FootPrint, placement.Rot, id);
                    placed++;
                }
            }
        }
    }

    private List<Placement> CollectPlacements(GridRoom section, IFurnitureFootPrint footprint)
    {
        var results = new List<Placement>();

        for (int x = section.X; x < section.X + section.Width; x++)
        {
            for (int z = section.Z; z < section.Z + section.Height; z++)
            {
                if (!IsWalkable(x, z) || HasFurniture(x, z))
                    continue;

                // Try 4 rotations
                for (int i = 0; i < 4; i++)
                {
                    Quaternion rot = Quaternion.Euler(0, i * 90, 0);
                    if (CanPlaceFootprint(new Vector2Int(x, z), footprint.FootPrint, rot))
                    {
                        ESpawnLocation type = DeterminePlacementType(new Vector2Int(x, z), footprint.FootPrint, rot);
                        results.Add(new Placement { Cell = new Vector2Int(x, z), Rot = rot, Type = type });
                    }
                }
            }
        }

        return results;
    }

    private ESpawnLocation DeterminePlacementType(Vector2Int anchor, Vector2Int size, Quaternion rot)
    {
        float y = rot.eulerAngles.y % 180f;
        bool swapped = Mathf.Abs(y - 90f) < 0.1f;
        int w = swapped ? size.y : size.x;
        int h = swapped ? size.x : size.y;

        ESpawnLocation bestType = ESpawnLocation.Center;

        for (int dz = 0; dz < h; dz++)
        {
            for (int dx = 0; dx < w; dx++)
            {
                ESpawnLocation cellType = DetermineLocationType(anchor.x + dx, anchor.y + dz);
                if (cellType == ESpawnLocation.Corner) return ESpawnLocation.Corner;
                if (cellType == ESpawnLocation.Wall) bestType = ESpawnLocation.Wall;
            }
        }
        return bestType;
    }

    private ESpawnLocation DetermineLocationType(int x, int z)
    {
        bool n = IsWallCell(x, z - 1);
        bool s = IsWallCell(x, z + 1);
        bool e = IsWallCell(x - 1, z);
        bool w = IsWallCell(x + 1, z);

        int wallCount = (n ? 1 : 0) + (s ? 1 : 0) + (e ? 1 : 0) + (w ? 1 : 0);

        if (wallCount >= 2 && ((n && e) || (n && w) || (s && e) || (s && w)))
            return ESpawnLocation.Corner;

        if (wallCount >= 1)
            return ESpawnLocation.Wall;

        return ESpawnLocation.Center;
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
        bool leftOrRight = UnityEngine.Random.value > 0.5f;

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
        bool leftOrRight = UnityEngine.Random.value > 0.5f;

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
        if (x < 0 || x >= gridCellIds.GetLength(0) || y < 0 || y >= gridCellIds.GetLength(1))
        {
            return;
        }

        gridCellIds[x, y] = roomId;
    }

    private void SpawnWalls()
    {
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
            }
        }
    }

    private bool IsWalkable(int x, int z)
    {
        int id = gridCellIds[x, z];
        return id == walkableId || id == corridorWalkableId;
    }

    private bool InBounds(int x, int z)
    {
        return x >= 0 && x < gridCellIds.GetLength(0) && z >= 0 && z < gridCellIds.GetLength(1);
    }

    private bool IsWall(int x, int z)
    {
        var cellId = gridCellIds[x, z];

        return cellId == northWallId ||
               cellId == eastWallId ||
               cellId == southWallId ||
               cellId == westWallId;
    }

    private bool IsWallCell(int x, int z)
    {
        if (!InBounds(x, z)) return true;
        return IsWall(x, z) || IsCornerId(gridCellIds[x, z]);
    }

    private struct Placement
    {
        public Vector2Int Cell;
        public Quaternion Rot;
        public ESpawnLocation Type;
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

    private bool HasFurniture(int x, int z)
    {
        int id = gridCellIds[x, z];
        return id >= furnitureBaseId;
    }

    private bool CanPlaceFootprint(Vector2Int anchor, Vector2Int size, Quaternion rot)
    {
        // if rotated 90 or 270 => swap
        float y = rot.eulerAngles.y % 180f;
        bool swapped = Mathf.Abs(y - 90f) < 0.1f;

        int w = swapped ? size.y : size.x;
        int h = swapped ? size.x : size.y;

        // Anchor convention: top-left, bottom-left, center, etc. Pick one and stay consistent.
        // Here: anchor is the MIN corner of the footprint (x..x+w-1, z..z+h-1).
        for (int dz = 0; dz < h; dz++)
        {
            for (int dx = 0; dx < w; dx++)
            {
                int x = anchor.x + dx;
                int z = anchor.y + dz;
                if (!InBounds(x, z))
                {
                    return false;
                }

                if (!IsWalkable(x, z))
                {
                    return false;
                }

                if (HasFurniture(x, z))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void MarkFurniture(Vector2Int anchor, Vector2Int size, Quaternion rot, int furnitureId)
    {
        float y = rot.eulerAngles.y % 180f;
        bool swapped = Mathf.Abs(y - 90f) < 0.1f;

        int w = swapped ? size.y : size.x;
        int h = swapped ? size.x : size.y;

        for (int dz = 0; dz < h; dz++)
        {
            for (int dx = 0; dx < w; dx++)
            {
                gridCellIds[anchor.x + dx, anchor.y + dz] = furnitureId;
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
                    case int n when n >= furnitureBaseId:
                        color = Color.green;
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
