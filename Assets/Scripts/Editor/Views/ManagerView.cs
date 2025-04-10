using System;
using Editor.Controllers;
using UnityEditor;
using Vector2 = UnityEngine.Vector2;

namespace Editor.Views
{
    public class ManagerView : EditorWindow
    {
        
        private InitializationView _initializationView;
        private SelectionView _selectionView;
        private SettingsView _settingsView;
        private StyleView _styleView;
        
        [MenuItem("Window/Generation Manager 2")]
        public static void ShowWindow()
        {
            var window = GetWindow<ManagerView>("Generation Manager 2");
            window.minSize = new Vector2(400, 600);
        }
        
        private void OnEnable()
        {
            InitializationController._onReload += Reload;
            InitializeDependencies();
            //_initializationManager.InitScene(); todo
        }

        private void OnDisable()
        {
            InitializationController._onReload -= Reload;
        }

        private void Reload()
        {
            CreateGUI();
        }

        private void InitializeDependencies()
        {
            //todo init
            _initializationView = new InitializationView();
            _selectionView = new SelectionView();
            _settingsView = new SettingsView();
            _styleView = new StyleView();
            
        }
        
        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();

            var scrollView = StyleUtils.SimpleScrollView();
            
            //todo add gui
            scrollView.Add(_initializationView.CreateUI());
            scrollView.Add(_selectionView.CreateUI());
            scrollView.Add(_settingsView.CreateUI());
            scrollView.Add(_styleView.CreateUI());

            
            root.Add(scrollView);
        }
    }
}