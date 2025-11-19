using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CorridorRoom : DungeonRoom
{
    public enum EOrientation
    {
        Horizontal,
        Vertical
    }

    private EOrientation orientation;

    public void InitializeCorridor(EOrientation orientation, Vector2Int size)
    {
        this.orientation = orientation;
        this.size = size;
    }

    protected override void GenerateWalls(Room room)
    {
        float halfWidth = room.Width / 2f;
        float halfLength = room.Length / 2f;
        float offset = 1f;
        float anotherOffset = 0.5f;

        if (orientation == EOrientation.Horizontal)
        {
            // Top
            GenerateWallLine(room, new Vector3(room.center.x - halfWidth, 0, room.center.z + halfLength + anotherOffset), Vector3.right, 0, size.x + 1);
            // Bottom
            GenerateWallLine(room, new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength - anotherOffset), Vector3.right, 0, size.x + 1);
        }
        else
        {
            // Left
            GenerateWallLine(room, new Vector3(room.center.x - halfWidth - anotherOffset, 0, room.center.z - halfLength - offset), Vector3.forward, 90, size.y + 1);
            // Right
            GenerateWallLine(room, new Vector3(room.center.x + halfWidth + anotherOffset, 0, room.center.z - halfLength - offset), Vector3.forward, 90, size.y + 1);
        }
    }

    protected override void GenerateInterior(Room room)
    {

    }
}
