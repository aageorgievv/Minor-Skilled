using UnityEngine;

public class KitchenRoom : DungeonRoom
{
    [Header("Interior/Furniture")]
    [SerializeField] private GameObject barPresetPrefab;
    [SerializeField] private GameObject[] barrelPrefabs;
    [SerializeField] private GameObject[] longTablePrefabs;
    [SerializeField] private GameObject chairPrefab;

    [Header("Generation settings")]
    [SerializeField] private int barSizeSpawn;
    [SerializeField] private int tableSizeSpawn;
    [SerializeField] private int BarrelSizeSpawn;
    [SerializeField, Range(1, 10)] private int barrelSpawnAmount;



    protected override void GenerateInterior(Room room)
    {
        GenerateBar(room, 0);
        GenerateTableAndChairs(room, 0);
        GenerateBarrels(room);
    }

    private void GenerateBar(Room room, int iteration)
    {
        if (iteration > maxPlacementIterations)
        {
            Debug.LogError("Fail safe while generating table/chairs");
            return;
        }

        if (room.Width >= barSizeSpawn && room.Length >= barSizeSpawn && barPresetPrefab != null)
        {
            int side = Random.Range(0, 4);
            float halfWidth = room.Width / 2f;
            float halfLength = room.Length / 2f;
            Vector3 barPosition = Vector3.zero;
            Quaternion barRotation = Quaternion.identity;

            switch (side)
            {
                case 0: // Left Center
                    barPosition = new Vector3(room.center.x - halfWidth, 1, room.center.z);
                    break;

                case 1: // Right Center
                    barPosition = new Vector3(room.center.x + halfWidth, 1, room.center.z);
                    barRotation = Quaternion.Euler(0, 180, 0);

                    break;

                case 2: // top-left
                    barPosition = new Vector3(room.center.x, 1, room.center.z + halfLength);
                    barRotation = Quaternion.Euler(0, 90, 0);

                    break;

                case 3: // top-right
                    barPosition = new Vector3(room.center.x, 1, room.center.z - halfLength);
                    barRotation = Quaternion.Euler(0, -90, 0);
                    break;
            }

            TryPlaceObject(barPresetPrefab, barPosition, barRotation);
        }
    }

    private void GenerateTableAndChairs(Room room, int iteration)
    {
        if (iteration > maxPlacementIterations)
        {
            Debug.LogError("Fail safe while generating table/chairs");
            return;
        }

        if (room.Width >= tableSizeSpawn && room.Length >= tableSizeSpawn && longTablePrefabs.Length > 0 && chairPrefab != null)
        {
            float offset = Random.Range(2, 5);
            float halfWidth = room.Width / 2f;
            float halfLength = room.Length / 2f;
            int tableSpawnAmount = Random.Range(0, longTablePrefabs.Length * 2);

            for (int i = 0; i < tableSpawnAmount; i++)
            {
                GameObject tablePrefab = longTablePrefabs[Random.Range(0, longTablePrefabs.Length)];
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

                    for (int j = 0; j < chairPositions.Length; j++)
                    {
                        TryPlaceObject(chairPrefab, chairPositions[j], chairOrientations[j]);
                    }
                }
            }
        }
    }

    private void GenerateBarrels(Room room)
    {
        if (room.Width >= BarrelSizeSpawn && room.Length != BarrelSizeSpawn && barrelPrefabs != null)
        {
            float halfWidth = room.Width / 2f;
            float halfLength = room.Length / 2f;
            float offset = 1.5f;
            int barrelSpawnAmount = Random.Range(0, this.barrelSpawnAmount + 1);

            for (int i = 0; i < barrelSpawnAmount; i++)
            {
                GameObject barrelPrefab = barrelPrefabs[Random.Range(0, longTablePrefabs.Length)];
                int wall = Random.Range(0, 4);
                Vector3 barrelPosition = Vector3.zero;
                Quaternion barrelRotation = Quaternion.identity;

                float randomX = Random.Range(room.center.x - halfWidth + offset, room.center.x + halfWidth - offset);
                float randomZ = Random.Range(room.center.z - halfLength + offset, room.center.z + halfLength - offset);

                switch (wall)
                {
                    case 0: // Top wall
                        barrelPosition = new Vector3(randomX, 0, room.center.z + halfLength - offset);
                        barrelRotation = Quaternion.Euler(0, 180, 0);
                        break;
                    case 1: // Bottom wall
                        barrelPosition = new Vector3(randomX, 0, room.center.z - halfLength + offset);
                        barrelRotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case 2: // Right wall
                        barrelPosition = new Vector3(room.center.x + halfWidth - offset, 0, randomZ);
                        barrelRotation = Quaternion.Euler(0, -90, 0);
                        break;
                    case 3: // Left wall
                        barrelPosition = new Vector3(room.center.x - halfWidth + offset, 0, randomZ);
                        barrelRotation = Quaternion.Euler(0, 90, 0);
                        break;
                }

                TryPlaceObject(barrelPrefab, barrelPosition, barrelRotation);
            }
        }
    }
}
