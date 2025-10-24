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
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        RunProceduralGeneration();
    }

    public void DeleteGeneration()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    protected abstract void RunProceduralGeneration();
}
