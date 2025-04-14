using UnityEditor;
using UnityEngine;
using EventBus = Models.Editor.EventBus;
using StyleUtils = Utils.StyleUtils;

namespace Views.Editor
{
    /// <summary>
    /// Represents the main editor window for the Generation Manager.
    /// Responsible for initializing and displaying various views related to generation management.
    /// </summary>
    public class ManagerView : EditorWindow
    {
        /// <summary>
        /// View responsible for initialization actions.
        /// </summary>
        private InitializationView _initializationView;

        /// <summary>
        /// View responsible for generator selection.
        /// </summary>
        private SelectionView _selectionView;

        /// <summary>
        /// View responsible for settings management.
        /// </summary>
        private SettingsView _settingsView;
        
        /// <summary>
        /// View responsible for sprite renaming actions.
        /// </summary>
        private SpriteRenamerView _spriteRenamerView;

        /// <summary>
        /// View responsible for style customization.
        /// </summary>
        private StyleView _styleView;

        /// <summary>
        /// View responsible for generation-related actions.
        /// </summary>
        private ActionsView _actionsView;

        /// <summary>
        /// Displays the Generation Manager window in the Unity Editor.
        /// </summary>
        [MenuItem("Window/Generation Manager 2")]
        public static void ShowWindow()
        {
            // Create and display the window with a minimum size.
            var window = GetWindow<ManagerView>("Generation Manager 2");
            window.minSize = new Vector2(400, 600);
        }

        /// <summary>
        /// Called when the window is enabled.
        /// Initializes dependencies, subscribes to events, and creates the UI.
        /// </summary>
        private void OnEnable()
        {
            InitializeDependencies();
            EventBus.Reload += Reload;
            EventBus.OnToolOpened();
        }

        /// <summary>
        /// Called when the window is disabled.
        /// Unsubscribes from events to prevent memory leaks.
        /// </summary>
        private void OnDisable()
        {
            EventBus.Reload -= Reload;
        }

        /// <summary>
        /// Reloads the UI by recreating it.
        /// </summary>
        private void Reload() => CreateGUI();

        /// <summary>
        /// Initializes the dependencies for the various views used in the window.
        /// </summary>
        private void InitializeDependencies()
        {
            _initializationView = new InitializationView();
            _selectionView = new SelectionView();
            _settingsView = new SettingsView();
            _spriteRenamerView = new SpriteRenamerView();
            _styleView = new StyleView();
            _actionsView = new ActionsView();
        }

        /// <summary>
        /// Creates and sets up the UI for the editor window.
        /// </summary>
        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();

            var scrollView = StyleUtils.SimpleScrollView();
            scrollView.Add(_initializationView.CreateUI());
            scrollView.Add(_selectionView.CreateUI());
            scrollView.Add(_settingsView.CreateUI());
            scrollView.Add(_spriteRenamerView.CreateUI());
            scrollView.Add(_styleView.CreateUI());
            scrollView.Add(_actionsView.CreateUI());

            root.Add(scrollView);
        }
    }
}