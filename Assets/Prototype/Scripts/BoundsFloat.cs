using UnityEngine;

[System.Serializable]
public struct BoundsFloat
{
    public Vector3 position;   // Center position
    public Vector3 size;       // Full size (not extents)

    public BoundsFloat(Vector3 position, Vector3 size)
    {
        this.position = position;
        this.size = size;
    }

    // --- Convenience Properties ---

    public Vector3 center
    {
        get => position;
        set => position = value;
    }

    public Vector3 extents
    {
        get => size * 0.5f;
        set => size = value * 2f;
    }

    public Vector3 min
    {
        get => position - extents;
        set => SetMinMax(value, max);
    }

    public Vector3 max
    {
        get => position + extents;
        set => SetMinMax(min, value);
    }

    // --- Methods ---

    public void SetMinMax(Vector3 min, Vector3 max)
    {
        size = max - min;
        position = min + size * 0.5f;
    }

    public void Encapsulate(Vector3 point)
    {
        Vector3 newMin = Vector3.Min(min, point);
        Vector3 newMax = Vector3.Max(max, point);
        SetMinMax(newMin, newMax);
    }

    public void Encapsulate(BoundsFloat other)
    {
        Vector3 newMin = Vector3.Min(min, other.min);
        Vector3 newMax = Vector3.Max(max, other.max);
        SetMinMax(newMin, newMax);
    }

    public bool Contains(Vector3 point)
    {
        return
            point.x >= min.x && point.x <= max.x &&
            point.y >= min.y && point.y <= max.y &&
            point.z >= min.z && point.z <= max.z;
    }

    public bool Intersects(BoundsFloat other)
    {
        return
            min.x <= other.max.x && max.x >= other.min.x &&
            min.y <= other.max.y && max.y >= other.min.y &&
            min.z <= other.max.z && max.z >= other.min.z;
    }

    public override string ToString()
    {
        return $"BoundsFloat(center: {center}, size: {size})";
    }
}
