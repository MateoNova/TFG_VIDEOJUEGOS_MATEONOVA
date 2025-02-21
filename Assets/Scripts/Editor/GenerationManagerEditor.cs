using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Ventana del editor para gestionar la generación de mazmorras.
    /// </summary>
    public class GenerationManagerWindow : EditorWindow
    {
        #region Fields

        private InitializationManager _initializationManager;
        private GeneratorSelection _generatorSelection;
        private GeneratorSettings _generatorSettings;
        private StyleManager _styleManager;
        private GenerationActions _generationActions;
        private Vector2 _globalScrollPosition;

        #endregion

        #region Initialization

        [MenuItem("Window/Generation Manager")]
        public static void ShowWindow()
        {
            GetWindow<GenerationManagerWindow>("Generation Manager");
        }

        private void OnEnable()
        {
            InitializeDependencies();
            _initializationManager.InitScene();
        }

        /// <summary>
        /// Initializes the dependencies of the window.
        /// </summary>
        private void InitializeDependencies()
        {
            _generatorSelection = new GeneratorSelection();
            _initializationManager = new InitializationManager(_generatorSelection);
            _generatorSettings = new GeneratorSettings(_generatorSelection);
            _styleManager = new StyleManager(_generatorSelection);
            _generationActions = new GenerationActions(_generatorSelection);
        }

        #endregion

        #region GUI Drawing

        /// <summary>
        /// Draws the GUI of the window.
        /// </summary>
        private void OnGUI()
        {
            _globalScrollPosition =
                EditorGUILayout.BeginScrollView(_globalScrollPosition, true, false, GUILayout.ExpandWidth(true));

            _initializationManager.Draw();
            _generatorSelection.Draw();
            _generatorSettings.Draw();
            _styleManager.Draw();
            _generationActions.Draw();

            EditorGUILayout.EndScrollView();
        }

        #endregion
    }
}