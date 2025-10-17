using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField] protected Vector3Int startPosition = Vector3Int.zero;

    [Header("References")]
    [SerializeField] protected DungeonRoom roomPrefab;
    [SerializeField] protected GameObject planePrefab;

    private void Start()
    {
        Debug.Log("Generating...");
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        RunProceduralGeneration();
    }

    protected abstract void RunProceduralGeneration();
}
