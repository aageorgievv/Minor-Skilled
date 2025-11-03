using UnityEngine;

public class Room
{
    public int FreeSpaces { get; private set; }
    public float Width => size.x;
    public float Length => size.y;

    public Vector3 center;
    private Vector2Int size;
    private bool[,] grid;


    public Room(Vector3 center, Vector2Int size)
    {
        this.center = center;
        this.size = size;
        grid = new bool[size.x, size.y];
        FreeSpaces = size.x * size.y;
    }

    public void Occupy(int x, int y)
    {
        if (!IsValidGridPosition(x, y))
        {
            return;
        }

        if (grid[x, y])
        {
            return;
        }

        grid[x, y] = true;
        FreeSpaces--;
    }

    public bool IsOccupied(int x, int y)
    {
        if (!IsValidGridPosition(x, y))
        {
            return false;
        }


        return grid[x, y];
    }

    private bool IsValidGridPosition(int x, int y)
    {
        if (x < 0 || y < 0)
        {
            return false;
        }

        if (x > size.x)
        {
            return false;
        }

        if (y > size.y)
        {
            return false;
        }

        return true;
    }
}
