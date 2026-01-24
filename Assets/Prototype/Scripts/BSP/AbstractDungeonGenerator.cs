using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    protected Vector3Int startPosition = Vector3Int.zero;

    [Header("References")]
    [SerializeField] protected DungeonRoom[] roomPrefabs;
    [SerializeField] protected GameObject bedPrefab;

    [SerializeField] protected GameObject cornerPrefab;
    [SerializeField] protected GameObject wallPrefab;

    private void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        RunProceduralGeneration();
    }

    public void DeleteDungeon()
    {
        DeleteProceduralGeneration();
    }

    protected abstract void RunProceduralGeneration();
    protected abstract void DeleteProceduralGeneration();
}
