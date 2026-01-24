using UnityEngine;

public class Bed : MonoBehaviour, IFurnitureFootPrint
{
    [SerializeField] private Vector2Int footPrint = new Vector2Int(2, 2);

    [SerializeField] private ESpawnLocation[] allowedLocations = { ESpawnLocation.Wall, ESpawnLocation.Corner };

    public Vector2Int FootPrint => footPrint;
    public ESpawnLocation[] AllowedLocations => allowedLocations;
}
