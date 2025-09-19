using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Vector3 center;
    public float width;
    public float length;

    public Room(Vector3 c, float w, float l)
    {
        center = c;
        width = w;
        length = l;
    }
}
