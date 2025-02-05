using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for the BaseGenerator class, providing a button to generate the dungeon in the Unity Inspector.
/// TODO: it will be deleted after the tool is done
/// </summary>
[CustomEditor(typeof(BaseGenerator), true)]
public class DungeonGeneratorEditor : UnityEditor.Editor
{
    /// <summary>
    /// Reference to the BaseGenerator instance being edited.
    /// </summary>
    private BaseGenerator _generator;
    
    /// <summary>
    /// Called when the editor is enabled. Initializes the _generator reference.
    /// </summary>
    private void OnEnable()
    {
        _generator = (BaseGenerator)target;
    }
    
    /// <summary>
    /// Overrides the default Inspector GUI to add a "Generate Dungeon" button.
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Dungeon"))
        {
            _generator.GenerateDungeon(); 
        } 
    }
}