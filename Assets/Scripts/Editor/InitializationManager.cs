using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Manages the initialization of the scene and the editor preferences.
    /// </summary>
    public class InitializationManager
    {
        #region Fields

        /// <summary>
        /// Indicates whether the initialization foldout should be shown.
        /// </summary>
        private bool _showInitialization = true;

        /// <summary>
        /// Reference to the GeneratorSelection instance.
        /// </summary>
        private readonly GeneratorSelection _generatorSelection;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationManager"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public InitializationManager(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        #region Drawing

        /// <summary>
        /// Draws the initialization foldout.
        /// </summary>
        public void Draw()
        {
            _showInitialization = EditorGUILayout.Foldout(_showInitialization, "Initialization", true,
                Utils.GetSectionTitleStyle());

            if (!_showInitialization) return;
            DrawButtons();
        }

        /// <summary>
        /// Draws the buttons for clearing and reinitializing the scene.
        /// </summary>
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

        #endregion

        #region ButtonsActions

        /// <summary>
        /// Clears the cached data and editor preferences.
        /// </summary>
        private void ClearCachedData()
        {
            EditorPrefs.DeleteAll();
            _generatorSelection.ClearCacheData();
        }

        /// <summary>
        /// Initializes the scene with the default values.
        /// </summary>
        public void InitScene()
        {
            _generatorSelection.RetrieveOrInitializeCachedGenerationManager();
            _generatorSelection.FindAllGenerators();
            _generatorSelection.SelectGenerator(0);
        }

        #endregion
    }
}