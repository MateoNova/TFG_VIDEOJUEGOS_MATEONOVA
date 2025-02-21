using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Editor window for managing dungeon generation.
    /// </summary>
    public class GenerationManagerWindow : EditorWindow
    {
        #region Constants and Fields

        private InitializationManager _initializationManager = InitializationManager.Instance;
        private GeneratorSelection _generatorSelection = GeneratorSelection.Instance;
        private GeneratorSettings _generatorSettings = GeneratorSettings.Instance;
        private StyleManager _styleManager = StyleManager.Instance;
        private GenerationActions _generationActions = GenerationActions.Instance;


        /// <summary>
        /// Flag to indicate whether the scene is initialized.
        /// </summary>
        private bool _isInitialized;


        /// <summary>
        /// Scroll position for the wall tile settings.
        /// </summary>
        private Vector2 _wallScrollPosition;

        #endregion


        private void InitScene()
        {
            _generatorSelection.RetrieveOrInitializeCachedGenerationManager();
            _generatorSelection.FindAllGenerators();
            //_isInitialized = true;
            _generatorSelection.SelectGenerator(0);
        }


        #region Initialization

        [MenuItem("Window/Generation Manager")]
        public static void ShowWindow()
        {
            GetWindow<GenerationManagerWindow>("Generation Manager");
        }


        private void OnEnable()
        {
            InitScene();
        }

        #endregion

        #region GUI Drawing

        private Vector2 _globalScrollPosition;

        private void OnGUI()
        {
            _globalScrollPosition =
                EditorGUILayout.BeginScrollView(_globalScrollPosition, true, false, GUILayout.ExpandWidth(true));

            _initializationManager.Draw();

            _generatorSelection.Draw();

            /*if (_generators == null || _generators.Count == 0)
            {
                EditorGUILayout.HelpBox("No generators found in the scene.", MessageType.Warning);
            }
            else
            {*/

            _generatorSettings.Draw();

            _styleManager.Draw();

            _generationActions.Draw();


            //}

            EditorGUILayout.EndScrollView();
        }

        #endregion
    }
}