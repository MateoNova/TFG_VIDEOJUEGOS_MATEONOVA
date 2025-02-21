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

        private readonly InitializationManager _initializationManager = InitializationManager.Instance;
        private readonly GeneratorSelection _generatorSelection = GeneratorSelection.Instance;
        private readonly GeneratorSettings _generatorSettings = GeneratorSettings.Instance;
        private readonly StyleManager _styleManager = StyleManager.Instance;
        private readonly GenerationActions _generationActions = GenerationActions.Instance;


        /// <summary>
        /// Flag to indicate whether the scene is initialized.
        /// </summary>
        private bool _isInitialized;


        /// <summary>
        /// Scroll position for the wall tile settings.
        /// </summary>
        private Vector2 _wallScrollPosition;

        #endregion


        #region Initialization

        [MenuItem("Window/Generation Manager")]
        public static void ShowWindow()
        {
            GetWindow<GenerationManagerWindow>("Generation Manager");
        }


        private void OnEnable()
        {
            _initializationManager.InitScene();
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
            else TODO RECORDAR REFACTORIZAR TODO LO RELACIONADO CON EL EDITOR PQ MUCHA SHIT Q PUEDO MEJORAR XD
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