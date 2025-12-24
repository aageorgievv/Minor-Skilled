using UnityEngine;

public class GridCell
{
    public Vector3 Position { get; }

    public int X;
    public int Z;
    public float Size;
    public int PrefabId;

    public GridCell(int x, int z, float size)
    {
        X = x;
        Z = z;
        Size = size;
        Position = new Vector3(x * Size + Size / 2f, 0, z * Size + Size / 2f);
    }
}
