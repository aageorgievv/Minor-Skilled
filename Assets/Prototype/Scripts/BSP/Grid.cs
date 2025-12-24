using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("Grid settings")]
    [SerializeField] private int xArea;
    [SerializeField] private int zArea;

    private float cellSize = 1f;
    private float gizmosCellSize = 0.9f;
    private GridCell[,] cells;

    public Vector3 GetCellCenterWorld(int x, int z)
    {
        return transform.position + cells[z, x].Position;
    }

    public void BuildGrid()
    {
        cells = new GridCell[zArea, xArea];

        for (int z = 0; z < zArea; z++)
        {
            for (int x = 0; x < xArea; x++)
            {
                cells[z, x] = new GridCell(x, z, cellSize);
            }
        }
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        // Called when you change values in the Inspector
        if (xArea < 1) xArea = 1;
        if (zArea < 1) zArea = 1;
        BuildGrid();
    }
#endif

    private void OnDrawGizmos()
    {
        if (cells == null)
        {
            return;
        }

        Gizmos.color = Color.green;

        Vector3 size = new Vector3(gizmosCellSize, 0.01f, gizmosCellSize);

        for (int z = 0; z < this.zArea; z++)
        {
            for (int x = 0; x < this.xArea; x++)
            {
                Vector3 center = transform.position + cells[z, x].Position;

                Gizmos.DrawCube(center, size);
            }
        }
    }
}
