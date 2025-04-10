using Editor.Controllers;
using Editor.Views;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// The main window for the Generation Manager.
    /// </summary>
    public class GenerationManagerWindow : EditorWindow
    {
        private InitializationManager _initializationManager;
        private GeneratorSelection _generatorSelection;
        private GeneratorSettings _generatorSettings;
        private StyleManager _styleManager;

        private ActionsView _actionsView;
        private ActionsController _actionsController;
        //private GenerationActions _generationActions;

        /// <summary>
        /// Opens the Generation Manager window.
        /// </summary>
        [MenuItem("Window/Generation Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<GenerationManagerWindow>("Generation Manager");
            window.minSize = new Vector2(400, 600);
        }

        /// <summary>
        /// Called when the window is enabled.
        /// Initializes dependencies and sets up the scene.
        /// </summary>
        private void OnEnable()
        {
            InitializeDependencies();
            _initializationManager.InitScene();
        }

        /// <summary>
        /// Initializes the dependencies for the window.
        /// </summary>
        private void InitializeDependencies()
        {
            _actionsView = new ActionsView();
            
            
            _generatorSelection = new GeneratorSelection();
            _initializationManager = new InitializationManager(_generatorSelection);
            _generatorSettings = new GeneratorSettings(_generatorSelection);
            _styleManager = new StyleManager(_generatorSelection);
            
            _initializationManager._onReload += CreateGUI;
        }

        /// <summary>
        /// Creates the user interface using UI Toolkit.
        /// </summary>
        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();

            var scrollView = StyleUtils.SimpleScrollView();

            scrollView.Add(_initializationManager.CreateUI());
            scrollView.Add(_generatorSelection.CreateUI());
            scrollView.Add(_generatorSettings.CreateUI());
            scrollView.Add(_styleManager.CreateUI());
            scrollView.Add(_actionsView.CreateUI(new ActionsController(_generatorSelection)));

            root.Add(scrollView);
        }
    }
}