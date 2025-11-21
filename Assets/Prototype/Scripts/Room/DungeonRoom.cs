using System.Collections.Generic;
using UnityEngine;

public abstract class DungeonRoom : MonoBehaviour
{
    [Header("Room Shell")]
    [SerializeField] protected GameObject unit1WallPrefab;
    [SerializeField] protected GameObject floorPrefab;

    [Header("Generation settings")]
    [SerializeField, Min(1f)] public Vector2Int size;

    protected const int maxPlacementIterations = 500;

    private Vector2Int lastSize;
    protected Room room;
    private LayerMask furnitureLayer;

    protected virtual void Start()
    {
        furnitureLayer = LayerMask.GetMask("Furniture");
    }

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

    public void StartGenerating()
    {
        room = new Room(Vector3.zero, size);

        GenerateWalls(room);
        GenerateFloor(room);
        GenerateInterior(room);
    }

    protected virtual void GenerateWalls(Room room)
    {
        float halfWidth = room.Width / 2f;
        float halfLength = room.Length / 2f;

        //Offsetting the walls to both avoid gaps in the corners and to align the grid properly(So the grid doesn't overlap with the walls)
        float offset = 1f;
        float anotherOffset = 0.5f;

        //Bottom
        GenerateWallLine(room, new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength - anotherOffset), Vector3.right, 0, size.x + 1);
        //Top
        GenerateWallLine(room, new Vector3(room.center.x - halfWidth, 0, room.center.z + halfLength + anotherOffset), Vector3.right, 0, size.x + 1);
        //Left
        GenerateWallLine(room, new Vector3(room.center.x - halfWidth - anotherOffset, 0, room.center.z - halfLength - offset), Vector3.forward, 90, size.y + 1);
        //Right
        GenerateWallLine(room, new Vector3(room.center.x + halfWidth + anotherOffset, 0, room.center.z - halfLength - offset), Vector3.forward, 90, size.y + 1);
    }

    protected void GenerateWallLine(Room room, Vector3 startPos, Vector3 dir, int angle, int totalLength)
    {
        int placedUnits = 0;

        if (unit1WallPrefab == null)
        {
            Debug.LogError("Missing wallPrefab1Unit! Cannot generate walls.");
            return;
        }

        while (placedUnits < totalLength)
        {
            int remaining = totalLength - placedUnits;
            int wallUnitSize = 1;
            float wallCenterOffset = wallUnitSize / 2f;
            Vector3 spawnPosition = startPos + dir * (placedUnits + wallCenterOffset);

            GameObject wall = Instantiate(unit1WallPrefab, transform);
            wall.transform.localPosition = spawnPosition;
            wall.transform.localRotation = Quaternion.Euler(0f, angle, 0f);

            //room.Occupy(gridX, gridZ);

            placedUnits += wallUnitSize;
        }
    }

    private void GenerateFloor(Room room)
    {
        GameObject floor = Instantiate(floorPrefab, transform);
        floor.transform.localPosition = room.center;
        floor.transform.localScale = new Vector3(room.Width, 0.1f, room.Length);
    }

    private void OnDrawGizmosSelected()
    {
        if (this.room == null)
        {
            return;
        }

        float halfWidth = room.Width / 2f;
        float halfLength = room.Length / 2f;
        Vector3 gridOrigin = new Vector3(-halfWidth, 0, -halfLength);
        Vector3 gizmoCellSize = new Vector3(0.9f, 0.1f, 0.9f);

        for (int x = 0; x < room.Width; x++)
        {
            for (int y = 0; y < room.Length; y++)
            {
                Vector3 cellCenterLocal = gridOrigin + new Vector3(x + 0.5f, 0, y + 0.5f);
                Vector3 cellCenterWorld = transform.TransformPoint(cellCenterLocal);

                if (room.IsOccupied(x, y))
                {
                    Gizmos.color = new Color(1f, 0, 0, 0.3f); // Red
                }
                else
                {
                    Gizmos.color = new Color(0, 1f, 0, 0.3f); // Green
                }

                Gizmos.DrawCube(cellCenterWorld, gizmoCellSize);
            }
        }
    }
    protected abstract void GenerateInterior(Room room);

    protected bool TryPlaceObjectInCorner(Room room, GameObject prefab, Vector2Int gridSize, Vector3 pivotOffest)
    {
        int corner = Random.Range(0, 4);
        Vector2Int gridPosition = Vector2Int.zero;
        Quaternion rotation = Quaternion.identity;

        Vector2Int sizeRot90 = GetRotatedSize(gridSize, Quaternion.Euler(0, 90, 0));
        Vector2Int sizeRot180 = GetRotatedSize(gridSize, Quaternion.Euler(0, 180, 0));
        Vector2Int sizeRot270 = GetRotatedSize(gridSize, Quaternion.Euler(0, -90, 0));

        switch (corner)
        {
            case 0: // bottom left
                gridPosition = new Vector2Int(0, 0);
                rotation = Quaternion.Euler(0, 90, 0);
                break;
            case 1: // bottom right
                gridPosition = new Vector2Int(size.x - gridSize.x, 0);
                rotation = Quaternion.Euler(0, 0, 0);
                break;
            case 2: // top left
                gridPosition = new Vector2Int(0, size.y - sizeRot180.y);
                rotation = Quaternion.Euler(0, 180, 0);
                break;
            case 3: // top right
                gridPosition = new Vector2Int(size.x - sizeRot270.x, size.y - sizeRot270.y);
                rotation = Quaternion.Euler(0, -90, 0);
                break;
        }

        return TryPlaceObjectOnGrid(room, prefab, gridSize, gridPosition, rotation, pivotOffest);
    }

    protected bool TryPlaceObjectAlongWall(Room room, GameObject prefab, Vector2Int gridSize, Vector3 pivotOffset, int padding)
    {
        int wall = Random.Range(0, 4);
        Vector2Int gridPosition = Vector2Int.zero;
        Quaternion rotation = Quaternion.identity;
        Vector2Int rotatedSize = gridSize;
        rotatedSize = GetRotatedSize(gridSize, rotation);

        switch (wall)
        {
            case 0: // Bottom wall
                rotation = Quaternion.Euler(0, 0, 0);
                if (size.x < rotatedSize.x + padding * 2)
                {
                    return false;
                }
                gridPosition.x = Random.Range(padding, size.x - rotatedSize.x - padding);
                gridPosition.y = 0;
                break;

            case 1: // Top wall
                rotation = Quaternion.Euler(0, 180, 0);
                if (size.x < rotatedSize.x + padding * 2)
                {
                    return false;
                }
                gridPosition.x = Random.Range(padding, size.x - rotatedSize.x - padding);
                gridPosition.y = size.y - rotatedSize.y;
                break;

            case 2: // Left wall
                rotation = Quaternion.Euler(0, 90, 0);
                if (size.y < rotatedSize.y + padding * 2)
                {
                    return false;
                }
                gridPosition.x = 0;
                gridPosition.y = Random.Range(padding, size.y - rotatedSize.y - padding);
                break;

            case 3: // Right wall
                rotation = Quaternion.Euler(0, -90, 0);
                if (size.y < rotatedSize.y + padding * 2)
                {
                    return false;
                }
                gridPosition.x = size.x - rotatedSize.x;
                gridPosition.y = Random.Range(padding, size.y - rotatedSize.y - padding);
                break;
        }

        return TryPlaceObjectOnGrid(room, prefab, gridSize, gridPosition, rotation, pivotOffset);
    }

    protected bool TryPlaceObjectInCenterArea(Room room, GameObject prefab, Vector2Int gridSize, Vector3 pivotOffset, int padding, out Vector2Int placedGridPos, out Quaternion placedRotation)
    {
        int randomRotation = Random.Range(0, 4);
        placedRotation = Quaternion.Euler(0, 90 * randomRotation, 0);
        Vector2Int rotatedSize = GetRotatedSize(gridSize, placedRotation);

        int xMin = padding;
        int xMax = size.x - rotatedSize.x - padding;
        int yMin = padding;
        int yMax = size.y - rotatedSize.y - padding;

        if (xMin > xMax || yMin > yMax)
        {
            Debug.Log($"Object {nameof(prefab)} cannot fit!");
            placedGridPos = Vector2Int.zero;
            return false;
        }

        placedGridPos = new Vector2Int(
            Random.Range(xMin, xMax + 1),
            Random.Range(yMin, yMax + 1)
        );

        return TryPlaceObjectOnGrid(room, prefab, gridSize, placedGridPos, placedRotation, pivotOffset);
    }

    protected bool TryPlaceObjectOnGrid(Room room, GameObject prefab, Vector2Int objectGridSize, Vector2Int gridPosition, Quaternion localRotation, Vector3 pivotOffset)
    {
        Vector2Int rotatedSize = GetRotatedSize(objectGridSize, localRotation);
        List<Vector2Int> cellsToOccupy = new List<Vector2Int>();

        for (int x = 0; x < rotatedSize.x; x++)
        {
            for (int y = 0; y < rotatedSize.y; y++)
            {
                Vector2Int cell = new Vector2Int(gridPosition.x + x, gridPosition.y + y);

                if (room.IsOccupied(cell.x, cell.y))
                {
                    return false;
                }

                cellsToOccupy.Add(cell);
            }
        }

        foreach (Vector2Int cell in cellsToOccupy)
        {
            room.Occupy(cell.x, cell.y);
        }

        Vector3 origin = GetGridOrigin();
        Vector3 anchorCornerPos = origin + new Vector3(gridPosition.x, 0, gridPosition.y);
        Vector3 objectCenterOffset = new Vector3(rotatedSize.x * 0.5f, 0, rotatedSize.y * 0.5f);
        Vector3 spawnPositionLocal = anchorCornerPos + objectCenterOffset;
        Vector3 finalSpawnPosition = spawnPositionLocal + (localRotation * pivotOffset);

        GameObject newObject = Instantiate(prefab, transform);
        newObject.transform.localPosition = finalSpawnPosition;
        newObject.transform.localRotation = localRotation;

        return true;
    }

    protected Vector3 GetGridOrigin()
    {
        return new Vector3(-room.Width / 2f, 0, -room.Length / 2f);
    }

    protected Vector2Int GetRotatedSize(Vector2Int size, Quaternion rotation)
    {
        float yAngle = Mathf.Round(rotation.eulerAngles.y);

        if (Mathf.Approximately(yAngle, 90f) || Mathf.Approximately(yAngle, 270f))
        {
            return new Vector2Int(size.y, size.x);
        }

        return size;
    }

    //Old Methods
    /*    protected bool TryPlaceObject(GameObject prefab, Vector3 localPosition, Quaternion localRotation)
        {
            if (prefab == null)
            {
                Debug.LogError($"The prefab: {nameof(prefab)} is null");
                return false;
            }

            BoxCollider collider = prefab.GetComponent<BoxCollider>();

            if (collider == null)
            {
                Debug.LogError($"The prefab: {nameof(prefab)} is missing a BoxCollider");
                return false;
            }

            Vector3 worldPosition = transform.TransformPoint(localPosition);
            Quaternion worldRotation = transform.rotation * localRotation;

            if (IsSpaceFree(worldPosition, collider.size * 0.5f, worldRotation, furnitureLayer))
            {
                GameObject newObject = Instantiate(prefab, transform);
                newObject.transform.localPosition = localPosition;
                newObject.transform.localRotation = localRotation;
                Physics.SyncTransforms();

                Debug.Log($"Placing {nameof(prefab)} at {localPosition}");
                return true;
            }

            return false;
        }

        private bool IsSpaceFree(Vector3 worldPosition, Vector3 halfExtents, Quaternion worldRotation, LayerMask layerMask)
        {
            Collider[] hits = Physics.OverlapBox(worldPosition, halfExtents, worldRotation, layerMask);
            return hits.Length == 0;
        }*/
}
