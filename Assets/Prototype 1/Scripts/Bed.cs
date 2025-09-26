using UnityEngine;

public class Bed : MonoBehaviour, ICanPlace
{
    [SerializeField] private Vector3Int size;

    public bool CanPlace(Vector3Int position, bool[,,] occupiedGridSpot)
    {
        for (int x = 0; x < size.x; x++)
        {
            for(int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    int gridX = position.x + x;
                    int gridY = position.y + y;
                    int gridZ = position.z + z;

                    if (gridX >= occupiedGridSpot.GetLength(0) || gridY >= occupiedGridSpot.GetLength(1) || gridZ >= occupiedGridSpot.GetLength(2))
                    {
                        return false;
                    }

                    if (occupiedGridSpot[gridX, gridY, gridZ])
                    {
                        return false;
                    }

                }
            }
        }
        return true;
    }

    public void Occupy(Vector3Int position, bool[,,] occupiedGridSpot)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    occupiedGridSpot[position.x + x, position.y + y, position.z + z] = true;
                }
            }
        }
    }
}
