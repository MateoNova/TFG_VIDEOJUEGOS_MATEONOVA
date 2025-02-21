using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class InitializationManager
    {
        private static InitializationManager _instance;

        public static InitializationManager Instance => _instance ??= new InitializationManager();
        private bool _showInitialization = true;

        private readonly GeneratorSelection _generatorSelection = GeneratorSelection.Instance;

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
                    ClearCachedData();
                }

                if (GUILayout.Button("Initialize Scene"))
                {
                    InitScene();
                }
            }
        }

        private void ClearCachedData()
        {
            EditorPrefs.DeleteAll();

            _generatorSelection.ClearCacheData();
            //EditorApplication.delayCall += Repaint;
        }

        internal void InitScene()
        {
            _generatorSelection.RetrieveOrInitializeCachedGenerationManager();
            _generatorSelection.FindAllGenerators();
            //_isInitialized = true;
            _generatorSelection.SelectGenerator(0);
        }
    }
}