using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(GenerationManager))]
    public class GenerationManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GenerationManager manager = (GenerationManager)target;

            if (!manager) return;
            manager.FindAllGenerators();
            // Obtener nombres de los generadores
            var generatorNames = manager.GetGeneratorNames();
            var selected = EditorGUILayout.Popup("Select Generator", manager.selectedGeneratorIndex, generatorNames.ToArray());
            manager.SelectGenerator(selected);

            // Si el usuario selecciona un generador diferente
            if (selected != manager.selectedGeneratorIndex)
            {
                manager.SelectGenerator(selected);
                EditorUtility.SetDirty(manager);
            }

            // Mostrar los parámetros del generador seleccionado
            if (manager.currentGenerator != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Generator Settings", EditorStyles.boldLabel);

                SerializedObject generatorObject = new SerializedObject(manager.currentGenerator);
                SerializedProperty property = generatorObject.GetIterator();
                property.NextVisible(true); // Saltar la primera propiedad (script)

                while (property.NextVisible(false))
                {
                    EditorGUILayout.PropertyField(property, true);
                }

                generatorObject.ApplyModifiedProperties();

                // Botón para generar el dungeon
                EditorGUILayout.Space();
                if (GUILayout.Button("Generate Dungeon"))
                {
                    manager.Generate();
                }
            }
        }
    }
}