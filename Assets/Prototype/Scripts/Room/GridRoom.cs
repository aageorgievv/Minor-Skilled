using System.Numerics;

public class GridRoom
{
    public int X { get; }
    public int Z { get; }
    public int Width { get; }
    public int Height { get; }

    public GridRoom(int x, int z, int width, int height)
    {
        X = x;
        Z = z;
        Width = width;
        Height = height;
    } 
}
