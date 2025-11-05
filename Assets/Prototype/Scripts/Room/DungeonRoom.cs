using System.Collections.Generic;
using UnityEngine;

public abstract class DungeonRoom : MonoBehaviour
{
    [Header("Room Shell")]
    [SerializeField] protected GameObject unit1WallPrefab;
    [SerializeField] protected GameObject doorPrefab;
    [SerializeField] protected GameObject floorPrefab;

    [Header("Generation settings")]
    [SerializeField, Min(1f)] public Vector2Int size;

    protected const int maxPlacementIterations = 500;

    private Vector2Int lastSize;
    private Room room;
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
        //Room currentRoom = new Room(Vector3.zero, size);
        room = new Room(Vector3.zero, size);

        GenerateWalls(room);
        GenerateFloor(room);
        GenerateInterior(room);
    }

    private void GenerateWalls(Room room)
    {
        /*        float maxStretch = 1.5f;
                float wallSize = 4f;*/

        float halfWidth = room.Width / 2f;
        float halfLength = room.Length / 2f;

        //Offsetting the walls to both avoid gaps in the corners and to align the grid properly(So the grid doesn't overlap with the walls)
        float offset = 1f;
        float anotherOffest = 0.5f;

        //Bottom
        GenerateWallLine(room, new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength - anotherOffest), Vector3.right, 0, size.x + 1);
        //Top
        GenerateWallLine(room, new Vector3(room.center.x - halfWidth, 0, room.center.z + halfLength + anotherOffest), Vector3.right, 0, size.x + 1);
        //Left
        GenerateWallLine(room, new Vector3(room.center.x - halfWidth - anotherOffest, 0, room.center.z - halfLength - offset), Vector3.forward, 90, size.y + 1);
        //Right
        GenerateWallLine(room, new Vector3(room.center.x + halfWidth + anotherOffest, 0, room.center.z - halfLength - offset), Vector3.forward, 90, size.y + 1);
    }


    //Stretching method

    /*    private void GenerateWallLine(Vector3 startPos, Vector3 dir, int angle, float totalLength, float maxStretch, float wallSize)
        {
            float placed = 0f;
            while (placed < totalLength)
            {
                float remaining = totalLength - placed;
                float maxCover = wallSize * maxStretch;
                float cover = Mathf.Min(remaining, maxCover);
                float scale = cover / wallSize;

                GameObject wallPrefab = wallPrefabs[Random.Range(0, wallPrefabs.Length)];
                GameObject wall = Instantiate(wallPrefab, transform);
                wall.transform.localPosition = startPos + dir * (placed + cover / 2);
                wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);
                wall.transform.localScale = new Vector3(scale, 1f, 1f);

                placed += cover;
            }
        }*/


    // 1UnitWall Method

    const int minimumFreeSpaces = 6;

    private void GenerateWallLine(Room room, Vector3 startPos, Vector3 dir, int angle, int totalLength)
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

/*            int gridX = Mathf.FloorToInt(spawnPosition.x);
            int gridZ = Mathf.FloorToInt(spawnPosition.z);

            if (room.FreeSpaces <= minimumFreeSpaces)
            {
                break;
            }

            if (room.IsOccupied(gridX, gridZ))
            {
                // this position in the room is occupied
                continue;
            }*/

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
    protected bool TryPlaceObject(GameObject prefab, Vector3 localPosition, Quaternion localRotation)
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

    protected abstract void GenerateInterior(Room room);

    private bool IsSpaceFree(Vector3 worldPosition, Vector3 halfExtents, Quaternion worldRotation, LayerMask layerMask)
    {
        Collider[] hits = Physics.OverlapBox(worldPosition, halfExtents, worldRotation, layerMask);
        return hits.Length == 0;
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
}
