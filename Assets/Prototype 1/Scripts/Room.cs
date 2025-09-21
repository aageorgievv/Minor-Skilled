using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public Vector3 center;
    public float width;
    public float length;

    public Room(Vector3 center, float width, float length)
    {
        this.center = center;
        this.width = width;
        this.length = length;
    }
}
