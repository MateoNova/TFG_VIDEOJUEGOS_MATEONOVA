using Editor.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Views
{
    public class ManagerView : EditorWindow
    {
        private InitializationView _initializationView;
        private SelectionView _selectionView;
        private SettingsView _settingsView;
        private StyleView _styleView;
        private ActionsView _actionsView;

        [MenuItem("Window/Generation Manager 2")]
        public static void ShowWindow()
        {
            ManagerView window = GetWindow<ManagerView>("Generation Manager 2");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            InitializeDependencies();
            // Se suscribe al evento de recarga desde el EventBus si es necesario
            EventBus.Reload += Reload;
            // Es una buena práctica reconstruir la UI cuando se inicializa
            CreateGUI();
        }

        private void OnDisable()
        {
            EventBus.Reload -= Reload;
        }

        private void Reload() => CreateGUI();

        private void InitializeDependencies()
        {
            _initializationView = new InitializationView();
            _selectionView = new SelectionView();
            _settingsView = new SettingsView();
            _styleView = new StyleView();
            _actionsView = new ActionsView();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            ScrollView scrollView = StyleUtils.SimpleScrollView();

            scrollView.Add(_initializationView.CreateUI());
            scrollView.Add(_selectionView.CreateUI());
            scrollView.Add(_settingsView.CreateUI());
            scrollView.Add(_styleView.CreateUI());
            scrollView.Add(_actionsView.CreateUI());

            root.Add(scrollView);
        }
    }
}