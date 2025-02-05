using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseGenerator), true)]
public class DungeonGeneratorEditor : Editor
{
    private BaseGenerator _generator;

    private void OnEnable()
    {
        _generator = (BaseGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Dungeon"))
        {
            _generator.GenerateDungeon(); 
        } 
    }
}

