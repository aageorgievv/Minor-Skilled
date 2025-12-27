using UnityEngine;

public static class Grid
{
    public static Vector3 ToWorldPosition(int x, int z, float gridSize)
    {
        return new Vector3(x * gridSize, 0, z * gridSize);
    }

    public static Vector3 ToWorldPositionCenter(int x, int z, float gridSize)
    {
        float halfGridSize = gridSize / 2f;
        return new Vector3(x * gridSize + halfGridSize, 0, z * gridSize + halfGridSize);
    }

    public static Vector2Int ToGridPosition(Vector3 worldPosition, float gridSize)
    {
        int x = Mathf.FloorToInt(worldPosition.x / gridSize);
        int z = Mathf.FloorToInt(worldPosition.z / gridSize);
        return new Vector2Int(x, z);
    }
}