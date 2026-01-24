using UnityEngine;
using System;

[Serializable]
public struct FurnitureConfig
{
    public GameObject prefab;
    public int count;
}

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    protected Vector3Int startPosition = Vector3Int.zero;

    [Header("References")]
    [SerializeField] protected DungeonRoom[] roomPrefabs;
    [SerializeField] protected FurnitureConfig[] furniturePrefabs;

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
