using System.Collections.Generic;
using UnityEngine;

public static class BinarySpacePartitioningAlgorithm
{
    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minXWidth, int minZWidth)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);

        while (roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();

            if (room.size.z >= minZWidth && room.size.x >= minXWidth)
            {
                if (Random.value < 0.5f)
                {
                    if (room.size.z >= minZWidth * 2)
                    {
                        SplitHorizontally(minZWidth, roomsQueue, room);
                    }
                    else if (room.size.x >= minXWidth * 2)
                    {
                        SplitVertically(minXWidth, roomsQueue, room);
                    }
                    else if (room.size.x >= minXWidth && room.z >= minZWidth)
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    if (room.size.x >= minXWidth * 2)
                    {
                        SplitVertically(minXWidth, roomsQueue, room);
                    }
                    else if (room.size.z >= minZWidth * 2)
                    {
                        SplitHorizontally(minZWidth, roomsQueue, room);
                    }
                    else if (room.size.x >= minXWidth && room.size.z >= minZWidth)
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }
        return roomsList;
    }

    private static void SplitVertically(int minXWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        var xSplit = Random.Range(minXWidth, room.size.x - minXWidth);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z), new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));

        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }

    private static void SplitHorizontally(int minZWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        var zSplit = Random.Range(minZWidth, room.size.z - minZWidth);
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, room.size.y, zSplit));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y, room.min.z + zSplit), new Vector3Int(room.size.x, room.size.y, room.size.z - zSplit));

        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }
}
