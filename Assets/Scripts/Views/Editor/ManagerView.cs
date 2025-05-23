﻿using UnityEditor;
using UnityEngine;
using EventBus = Models.Editor.EventBus;
using StyleUtils = Utils.StyleUtils;

#if UNITY_EDITOR

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
        private SpriteProcessView spriteProcessView;

        /// <summary>
        /// View responsible for style customization.
        /// </summary>
        private StyleView _styleView;

        /// <summary>
        /// View responsible for generation-related actions.
        /// </summary>
        private ActionsView _actionsView;

        /// <summary>
        /// View responsible for spawning actions.
        /// </summary>
        private SpawningView _spawningView;

        /// <summary>
        /// Displays the Generation Manager window in the Unity Editor.
        /// </summary>
        [MenuItem("Window/Generation Manager V.2")]
        public static void ShowWindow()
        {
            var window = GetWindow<ManagerView>("Generation Manager V.2");
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
            spriteProcessView = new SpriteProcessView();
            _styleView = new StyleView();
            _actionsView = new ActionsView();
            _spawningView = new SpawningView();
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
            scrollView.Add(spriteProcessView.CreateUI());
            scrollView.Add(_styleView.CreateUI());
            scrollView.Add(_actionsView.CreateUI());
            scrollView.Add(_spawningView.CreateUI());

            root.Add(scrollView);
        }
    }
}

# endif