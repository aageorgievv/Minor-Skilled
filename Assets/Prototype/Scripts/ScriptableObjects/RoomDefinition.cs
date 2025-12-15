using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Room Definition")]
public class RoomDefinition : ScriptableObject
{
    public ERoomType roomType;
    public DungeonRoom prefab;

    [Header("Amount")]
    public int guaranteedAmount;

    [Header("Fallback Settings")]
    public bool canBeFiller = false;


}
