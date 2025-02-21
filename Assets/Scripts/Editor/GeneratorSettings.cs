using Editor;
using UnityEditor;

public class GeneratorSettings
{
    private static GeneratorSettings _instance;
    private bool _showGeneratorSettings = true;

    private GeneratorSelection _generatorSelection = GeneratorSelection.Instance;


    public static GeneratorSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GeneratorSettings();
            }

            return _instance;
        }
    }

    public void Draw()
    {
        _showGeneratorSettings = EditorGUILayout.Foldout(_showGeneratorSettings, "Generator Settings", true);
        if (_showGeneratorSettings)
        {
            DrawGeneratorSettings();
        }
    }

    private void DrawGeneratorSettings()
    {
        if (!_generatorSelection._currentGenerator) return;

        using (new EditorGUILayout.VerticalScope("box"))
        {
            SerializedObject generatorObject = new(_generatorSelection._currentGenerator);
            var property = generatorObject.GetIterator();
            property.NextVisible(true);

            while (property.NextVisible(false))
            {
                if (Utils.ShouldDisplayField(generatorObject, property.name))
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            generatorObject.ApplyModifiedProperties();
        }
    }
}