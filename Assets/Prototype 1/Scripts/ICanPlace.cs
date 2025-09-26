using UnityEngine;

public interface ICanPlace
{
    public bool CanPlace(Vector3Int gridPosition, bool[,,] occupiedGridSpot);
    public void Occupy(Vector3Int gridPosition, bool[,,] occupiedGridSpot);
}
