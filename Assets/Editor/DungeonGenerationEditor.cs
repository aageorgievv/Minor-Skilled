using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbstractDungeonGenerator), true)]
public class DungeonGenerationEditor : Editor
{
    private AbstractDungeonGenerator generator;

    private void Awake()
    {
        generator = (AbstractDungeonGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Generate Dungeon"))
        {
            generator.GenerateDungeon();
        }

        if(GUILayout.Button("Delete Generation"))
        {
            generator.DeleteGeneration();
        }
    }
}
