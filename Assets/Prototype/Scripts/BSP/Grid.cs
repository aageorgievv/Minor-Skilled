using UnityEngine;

public class Grid : MonoBehaviour
{
    private readonly GridCell[,] cells;
    private readonly float gridSize;

    public Grid(int width, int height, float gridSize)
    {
        this.gridSize = gridSize;

        for (int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                cells[i, j] = new GridCell(i, j, gridSize);
            }
        }
    }

    public Vector3 GetCellPosition(int x, int z) {
        return transform.position + cells[z, x].Position;
    }
}
