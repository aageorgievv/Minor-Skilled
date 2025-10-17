using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonRoom : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private GameObject[] wallPrefabs;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject floorPrefab;

    [Header("Interior/Furniture")]
    [SerializeField] private GameObject[] longTablePrefabs;
    [SerializeField] private GameObject[] bedPrefabs;
    [SerializeField] private GameObject chairPrefab;

    [Header("Treasure/Loot")]
    [SerializeField] private GameObject emptyChestPrefab;
    [SerializeField] private GameObject treasureChestPrefab;

    [Header("Generation settings")]
    [SerializeField, Min(1f)] public Vector2 size;
    [SerializeField] private int tableSizeSpawn = 6;
    [SerializeField] private int chestSizeSpawn = 3;
    [SerializeField] private int bedSizeSpawn = 2;


    [Header("Settings")]
    [SerializeField, Range(1, 5)] private int maxChestSpawnAmount = 3;


    protected List<Room> rooms = new List<Room>();
    protected List<Door> doors = new List<Door>();

    private Vector2 lastSize;

    private bool[,,] grid;
    private int gridWidth;
    private int gridHeight;
    private int gridLenght;

    private LayerMask furnitureLayer;

    private const int maxBedIterations = 500;

    private void Start()
    {
        furnitureLayer = LayerMask.GetMask("Furniture");
    }

/*    private void Update()
    {
        if (size != lastSize)
        {
            InitializeGrid(new Vector3(size.x, 5, size.y));

            lastSize = size;

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            StartGenerating();
        }
    }*/

    public void StartGenerating()
    {
        rooms.Clear();
        doors.Clear();

        Room dungeonRoom = new Room(Vector3.zero, size.x, size.y);
        rooms.Add(dungeonRoom);

        GenerateRooms();
    }

    private void GenerateRooms()
    {
        foreach (Room room in rooms)
        {
            GenerateWalls(room);
            GenerateFloor(room);
            GenerateInterior(room);
        }
    }

    private void GenerateWalls(Room room)
    {
        float maxStretch = 1.5f;
        float wallSize = 4f;

        float halfWidth = room.width / 2f;
        float halfLength = room.length / 2f;

        //Bottom
        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength), Vector3.right, 0, room.width, maxStretch, wallSize);
        //Top
        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z + halfLength), Vector3.right, 0, room.width, maxStretch, wallSize);
        //Left
        GenerateWallLine(new Vector3(room.center.x - halfWidth, 0, room.center.z - halfLength), Vector3.forward, 90, room.length, maxStretch, wallSize);
        //Right
        GenerateWallLine(new Vector3(room.center.x + halfWidth, 0, room.center.z - halfLength), Vector3.forward, 90, room.length, maxStretch, wallSize);
    }

    private void GenerateWallLine(Vector3 startPos, Vector3 dir, int angle, float totalLength, float maxStretch, float wallSize)
    {
        float placed = 0f;
        while (placed < totalLength)
        {
            float remaining = totalLength - placed;
            float maxCover = wallSize * maxStretch;
            float cover = Mathf.Min(remaining, maxCover);
            float scale = cover / wallSize;

            GameObject wallPrefab = wallPrefabs[Random.Range(0, wallPrefabs.Length)];
            GameObject wall = Instantiate(wallPrefab, transform);
            wall.transform.localPosition = startPos + dir * (placed + cover / 2);
            wall.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            wall.transform.localScale = new Vector3(scale, 1f, 1f);

            placed += cover;
        }
    }

    private void GenerateFloor(Room room)
    {
        GameObject floor = Instantiate(floorPrefab, transform);
        //floor.transform.position = room.center;
        floor.transform.localPosition = room.center;
        floor.transform.localScale = new Vector3(room.width, 0.1f, room.length);
    }

    private void GenerateInterior(Room room)
    {
        GenerateTableAndChairs(room, 0);
        GenerateBeds(room, 0);
        GenerateChest(room);
    }

    private void GenerateTableAndChairs(Room room, int iteration)
    {
        if (iteration > maxBedIterations)
        {
            Debug.LogError("Fail safe while generating table/chairs");
            return;
        }

        if (room.width >= tableSizeSpawn && room.length >= tableSizeSpawn && longTablePrefabs.Length > 0 && chairPrefab != null)
        {
            GameObject tablePrefab = longTablePrefabs[Random.Range(0, longTablePrefabs.Length)];
            Vector3 tableSize = tablePrefab.GetComponent<BoxCollider>().size;

            float offset = Random.Range(5, 10);
            float halfWidth = room.width / 2f;
            float halfLength = room.length / 2f;

            float randomX = Random.Range(room.center.x - halfWidth + offset, room.center.x + halfWidth - offset);
            float randomZ = Random.Range(room.center.z - halfLength + offset, room.center.z + halfLength - offset);

            Vector3 randomPosition = new Vector3(randomX, 0, randomZ);

            float chairOffsetX = 1.5f;
            float chairOffsetZ = 1f;
            Vector3[] chairPositions =
            {
                randomPosition + new Vector3(chairOffsetX, 0, -chairOffsetZ),
                randomPosition + new Vector3(chairOffsetX, 0, chairOffsetZ),
                randomPosition + new Vector3(-chairOffsetX, 0, -chairOffsetZ),
                randomPosition + new Vector3(-chairOffsetX, 0, chairOffsetZ),
            };

            Quaternion[] chairOrientations =
            {
                Quaternion.Euler(0, 180, 0),
                Quaternion.Euler(0, 180, 0),
                Quaternion.Euler(0, 0, 0),
                Quaternion.Euler(0, 0, 0),

            };

            if (IsSpaceFree(randomPosition, tableSize, Quaternion.identity, furnitureLayer))
            {
                //Instantiate(tablePrefab, randomPosition, Quaternion.identity, transform);
                var table = Instantiate(tablePrefab, transform);
                table.transform.localPosition = randomPosition;
                table.transform.localRotation = Quaternion.identity;

                for (int i = 0; i < chairPositions.Length; i++)
                {
                    //Instantiate(chairPrefab, chairPositions[i], chairOrientations[i], transform);
                    var chair = Instantiate(chairPrefab, transform);
                    chair.transform.localPosition = chairPositions[i];
                    chair.transform.localRotation = chairOrientations[i];
                }
            }
            else
            {
                GenerateTableAndChairs(room, ++iteration);
                Debug.LogError($"Cant place {nameof(tablePrefab)} at {randomPosition}");
            }
        }
    }

    private void GenerateChest(Room room)
    {
        if (room.width >= chestSizeSpawn && room.length != chestSizeSpawn && emptyChestPrefab != null && treasureChestPrefab != null)
        {
            GameObject chestPrefab = Random.value < 0.5f ? emptyChestPrefab : treasureChestPrefab;
            Vector3 chestSize = chestPrefab.GetComponent<BoxCollider>().size;

            float halfWidth = room.width / 2f;
            float halfLength = room.length / 2f;
            float offset = 1f;
            int chestSpawnAmount = Random.Range(0, maxChestSpawnAmount + 1);

            for (int i = 0; i < chestSpawnAmount; i++)
            {
                int wall = Random.Range(0, 4);
                Vector3 chestPosition = Vector3.zero;
                Quaternion chestRotation = Quaternion.identity;

                float randomX = Random.Range(room.center.x - halfWidth + offset, room.center.x + halfWidth - offset);
                float randomZ = Random.Range(room.center.z - halfLength + offset, room.center.z + halfLength - offset);

                switch (wall)
                {
                    case 0: // Top wall
                        chestPosition = new Vector3(randomX, 0, room.center.z + halfLength - offset);
                        chestRotation = Quaternion.Euler(0, 180, 0);
                        break;
                    case 1: // Bottom wall
                        chestPosition = new Vector3(randomX, 0, room.center.z - halfLength + offset);
                        chestRotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case 2: // Right wall
                        chestPosition = new Vector3(room.center.x + halfWidth - offset, 0, randomZ);
                        chestRotation = Quaternion.Euler(0, -90, 0);
                        break;
                    case 3: // Left wall
                        chestPosition = new Vector3(room.center.x - halfWidth + offset, 0, randomZ);
                        chestRotation = Quaternion.Euler(0, 90, 0);
                        break;
                }

                if (IsSpaceFree(chestPosition, chestSize, chestRotation, furnitureLayer))
                {
                    //Instantiate(chestPrefab, chestPosition, chestRotation, transform);
                    var chest = Instantiate(chestPrefab, transform);
                    chest.transform.localPosition = chestPosition;
                    chest.transform.localRotation = chestRotation;
                }
                else
                {
                    Debug.LogError($"Cant place {nameof(chairPrefab)} at {chestPosition}");
                }
            }
        }
    }

    private void GenerateBeds(Room room, int iteration)
    {
        if (iteration > maxBedIterations)
        {

            return;
        }

        if (room.width >= bedSizeSpawn && room.length != bedSizeSpawn && bedPrefabs.Length > 0)
        {
            GameObject bedPrefab = bedPrefabs[Random.Range(0, bedPrefabs.Length)];
            Vector3 bedsize = bedPrefab.GetComponent<BoxCollider>().size;

            float halfWidth = room.width / 2f;
            float halfLength = room.length / 2f;
            float offset1 = 1.5f;
            float offset2 = 2f;

            int corner = Random.Range(0, 4);
            Vector3 bedPosition = Vector3.zero;
            Quaternion bedRotation = Quaternion.identity;

            switch (corner)
            {
                case 0: // bottom-left
                    bedPosition = new Vector3(room.center.x - halfWidth + offset2, 0, room.center.z - halfLength + offset1);
                    bedRotation = Quaternion.Euler(0, 90, 0);
                    break;

                case 1: // bottom-right
                    bedPosition = new Vector3(room.center.x + halfWidth - offset1, 0, room.center.z - halfLength + offset2);
                    bedRotation = Quaternion.Euler(0, 0, 0);
                    break;

                case 2: // top-left
                    bedPosition = new Vector3(room.center.x - halfWidth + offset1, 0, room.center.z + halfLength - offset2);
                    bedRotation = Quaternion.Euler(0, 180, 0);
                    break;

                case 3: // top-right
                    bedPosition = new Vector3(room.center.x + halfWidth - offset2, 0, room.center.z + halfLength - offset1);
                    bedRotation = Quaternion.Euler(0, -90, 0);
                    break;
            }

            if (IsSpaceFree(bedPosition, bedsize, bedRotation, furnitureLayer))
            {
                //Instantiate(bedPrefab, bedPosition, bedRotation, transform);
                var bed = Instantiate(bedPrefab, transform);
                bed.transform.localPosition = bedPosition;
                bed.transform.localRotation = bedRotation;
            }
            else
            {
                GenerateBeds(room, ++iteration);
                Debug.LogError($"Cant place {nameof(bedPrefab)} at {bedPosition}");
            }
        }
    }

    private bool IsSpaceFree(Vector3 position, Vector3 size, Quaternion rotation, LayerMask layerMask)
    {
        Collider[] hits = Physics.OverlapBox(position, size, rotation, layerMask);

        return hits.Length == 0;
    }
}
