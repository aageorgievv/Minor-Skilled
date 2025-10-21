using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    protected Vector3Int startPosition = Vector3Int.zero;

    [Header("References")]
    [SerializeField] protected DungeonRoom[] roomPrefabs;

    private void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        RunProceduralGeneration();
    }

    protected abstract void RunProceduralGeneration();
}
