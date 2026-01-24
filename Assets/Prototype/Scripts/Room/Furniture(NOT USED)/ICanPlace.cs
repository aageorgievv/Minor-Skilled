using UnityEngine;

public interface IFurnitureFootPrint
{
    Vector2Int FootPrint {  get; }
    ESpawnLocation[] AllowedLocations { get; }
}
