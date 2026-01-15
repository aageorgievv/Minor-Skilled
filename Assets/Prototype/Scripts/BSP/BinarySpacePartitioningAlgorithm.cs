using System.Collections.Generic;
using UnityEngine;

//New

public static class BinarySpacePartitioningAlgorithm
{
    public static bool SplitRoom(GridRoom room, int minXWidth, int minZWidth, out GridRoom roomA, out GridRoom roomB)
    {
        bool canSplitHorizontally = room.Width >= minXWidth * 2;
        bool canSplitVertically = room.Height >= minZWidth * 2;

        if (!canSplitHorizontally && !canSplitVertically)
        {
            roomA = null;
            roomB = null;
            return false;
        }

        if (Random.value < 0.5f)
        {
            return SplitHorizontally(room, minXWidth, out roomA, out roomB);
        }
        else
        {
            return SplitVertically(room, minZWidth, out roomA, out roomB);
        }
    }

    private static bool SplitVertically(GridRoom room, int minZWidth, out GridRoom roomA, out GridRoom roomB)
    {
        if (room.Height < minZWidth * 2)
        {
            roomA = null;
            roomB = null;
            return false;
        }

        int splitZ = Random.Range(minZWidth, room.Height - minZWidth);
        roomA = new GridRoom(room.X, room.Z, room.Width, splitZ);
        roomB = new GridRoom(room.X, room.Z + splitZ, room.Width, room.Height - splitZ);
        return true;
    }

    private static bool SplitHorizontally(GridRoom room, int minXWidth, out GridRoom roomA, out GridRoom roomB)
    {
        if (room.Width < minXWidth * 2)
        {
            roomA = null;
            roomB = null;
            return false;
        }

        int splitX = Random.Range(minXWidth, room.Width - minXWidth);
        roomA = new GridRoom(room.X, room.Z, splitX, room.Height);
        roomB = new GridRoom(room.X + splitX, room.Z, room.Width - splitX, room.Height);
        return true;
    }


    public static bool SplitRoom(GridRoom room, Queue<GridRoom> rooms, int minXWidth, int minZWidth)
    {
        bool canSplitHorizontally = room.Width >= minXWidth * 2;
        bool canSplitVertically = room.Height >= minZWidth * 2;

        if(!canSplitHorizontally && !canSplitVertically)
        {
            return false;
        }

        if (Random.value < 0.5f)
        {
            return SplitHorizontally(room, minXWidth, rooms);
        }
        else
        {
            return SplitVertically(room, minZWidth, rooms);
        }
    }

    private static bool SplitVertically(GridRoom room, int minZWidth, Queue<GridRoom> rooms)
    {
        if(room.Height < minZWidth * 2)
        {
            return false;
        }

        int splitZ = Random.Range(minZWidth, room.Height - minZWidth);

        GridRoom roomA = new GridRoom(room.X, room.Z, room.Width, splitZ);
        GridRoom roomB = new GridRoom(room.X, room.Z + splitZ, room.Width, room.Height - splitZ);

        rooms.Enqueue(roomA);
        rooms.Enqueue(roomB);
        return true;
    }

    private static bool SplitHorizontally(GridRoom room, int minXWidth, Queue<GridRoom> rooms)
    {
        if(room.Width < minXWidth * 2)
        {
            return false;
        }

        int splitX = Random.Range(minXWidth, room.Width - minXWidth);

        GridRoom roomA = new GridRoom(room.X, room.Z, splitX, room.Height);
        GridRoom roomB = new GridRoom(room.X + splitX, room.Z, room.Width - splitX, room.Height);

        rooms.Enqueue(roomA);
        rooms.Enqueue(roomB);

        return true;
    }

    //Old

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
                    else if (room.size.x >= minXWidth && room.size.z >= minZWidth)
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
