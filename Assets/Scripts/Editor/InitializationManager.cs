using UnityEditor;
using UnityEngine;

public class InitializationManager
{
    
    private static InitializationManager _instance;

    public static InitializationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new InitializationManager();
            }
            return _instance;
        }
    }
    private bool _showInitialization = true;

    public void Draw()
    {
        _showInitialization = EditorGUILayout.Foldout(_showInitialization, "Initialization", true);
            if (_showInitialization)
            {
                DrawButtons();
            }
    }
    
    private void DrawButtons()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Clear and delete"))
            {
                //ClearCachedData();
            }

            if (GUILayout.Button("Initialize Scene"))
            {
                //InitScene();
            }
        }
    }
}
