using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Room Set")]
public class RoomSet : ScriptableObject
{
    public List<RoomDefinition> entries;
}
