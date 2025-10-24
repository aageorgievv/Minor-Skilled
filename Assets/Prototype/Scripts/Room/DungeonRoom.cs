using System.Collections.Generic;
using UnityEngine;

public abstract class DungeonRoom : MonoBehaviour
{
    [Header("Room Shell")]
    [SerializeField] protected GameObject[] wallPrefabs;
    [SerializeField] protected GameObject doorPrefab;
    [SerializeField] protected GameObject floorPrefab;

    [Header("Generation settings")]
    [SerializeField, Min(1f)] public Vector2 size;

    protected const int maxPlacementIterations = 500;
    protected List<Room> rooms = new List<Room>();
    protected List<Door> doors = new List<Door>();

    private Vector2 lastSize;
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
        rooms.Clear();
        doors.Clear();

        Room currentRoom = new Room(Vector3.zero, size.x, size.y);
        rooms.Add(currentRoom);

        GenerateWalls(currentRoom);
        GenerateFloor(currentRoom);
        GenerateInterior(currentRoom);
    }

    private void GenerateWalls(Room room)
    {
        float maxStretch = 1.5f;
        float wallSize = 4f;

        float halfWidth = room.width / 2f;
        float halfLength = room.length / 2f;

        //Bottom
        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength), Vector3.right, 0, room.width, maxStretch, wallSize);
        //Top
        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z + halfLength), Vector3.right, 0, room.width, maxStretch, wallSize);
        //Left
        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength), Vector3.forward, 90, room.length, maxStretch, wallSize);
        //Right
        GenerateWallLine(new Vector3(room.center.x + halfWidth, 0, room.center.z - halfLength), Vector3.forward, 90, room.length, maxStretch, wallSize);
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

            GameObject wallPrefab = wallPrefabs[Random.Range(0, wallPrefabs.Length)];
            GameObject wall = Instantiate(wallPrefab, transform);
            wall.transform.localPosition = startPos + dir * (placed + cover / 2);
            wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            wall.transform.localScale = new Vector3(scale, 1f, 1f);

            placed += cover;
        }
    }

    private void GenerateFloor(Room room)
    {
        GameObject floor = Instantiate(floorPrefab, transform);
        floor.transform.localPosition = room.center;
        floor.transform.localScale = new Vector3(room.width, 0.1f, room.length);
    }

    protected bool TryPlaceObject(GameObject prefab, Vector3 localPosition, Quaternion localRotation)
    {
        if(prefab == null)
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

        if(IsSpaceFree(worldPosition, collider.size * 0.5f, worldRotation, furnitureLayer))
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
}
