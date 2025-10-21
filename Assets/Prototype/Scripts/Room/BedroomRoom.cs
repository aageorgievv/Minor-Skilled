using System.Collections.Generic;
using UnityEngine;

public class BedroomRoom : DungeonRoom
{
    [Header("Interior/Furniture")]
    [SerializeField] private GameObject[] longTablePrefabs;
    [SerializeField] private GameObject[] bedPrefabs;
    [SerializeField] private GameObject chairPrefab;

    [Header("Treasure/Loot")]
    [SerializeField] private GameObject emptyChestPrefab;
    [SerializeField] private GameObject treasureChestPrefab;

    [Header("Generation settings")]
    [SerializeField] private int tableSizeSpawn;
    [SerializeField] private int chestSizeSpawn;
    [SerializeField] private int bedSizeSpawn;
    [SerializeField, Range(1, 5)] private int maxChestSpawnAmount;

    private const int maxBedIterations = 500;

    protected override void GenerateInterior(Room room)
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
            BoxCollider boxCollider = tablePrefab.GetComponent<BoxCollider>();

            float offset = Random.Range(5, 10);
            float halfWidth = room.width / 2f;
            float halfLength = room.length / 2f;

            float randomX = Random.Range(room.center.x - halfWidth + offset, room.center.x + halfWidth - offset);
            float randomZ = Random.Range(room.center.z - halfLength + offset, room.center.z + halfLength - offset);
            Vector3 randomPosition = new Vector3(randomX, 0, randomZ);

            if (TryPlaceObject(tablePrefab, randomPosition, Quaternion.identity))
            {
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

                for (int i = 0; i < chairPositions.Length; i++)
                {
                    TryPlaceObject(chairPrefab, chairPositions[i], chairOrientations[i]);
                }
            }
            else
            {
                GenerateTableAndChairs(room, ++iteration);
            }
        }
    }

    private void GenerateChest(Room room)
    {
        if (room.width >= chestSizeSpawn && room.length != chestSizeSpawn && emptyChestPrefab != null && treasureChestPrefab != null)
        {
            GameObject chestPrefab = Random.value < 0.5f ? emptyChestPrefab : treasureChestPrefab;

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

                TryPlaceObject(chestPrefab, chestPosition, chestRotation);
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
            BoxCollider boxCollider = bedPrefab.GetComponent<BoxCollider>();

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

            if (!TryPlaceObject(bedPrefab, bedPosition, bedRotation))
            {
                GenerateBeds(room, ++iteration);
            }
        }
    }

/*    //Debug
    public struct Placement
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 size;
    }

    private readonly List<Placement> placements = new List<Placement>();
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (Placement placement in placements)
        {
            Gizmos.DrawCube(placement.position, placement.size);
        }
    }*/
}
