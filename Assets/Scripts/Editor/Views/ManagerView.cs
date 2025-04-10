using UnityEditor;
using Vector2 = UnityEngine.Vector2;

namespace Editor.Views
{
    public class ManagerView : EditorWindow
    {
        
        private InitializationView _initializationView;
        
        [MenuItem("Window/Generation Manager 2")]
        public static void ShowWindow()
        {
            var window = GetWindow<ManagerView>("Generation Manager 2");
            window.minSize = new Vector2(400, 600);
        }
        
        private void OnEnable()
        {
            InitializeDependencies();
            //_initializationManager.InitScene(); todo
        }

        private void InitializeDependencies()
        {
            //todo init
            _initializationView = new InitializationView();
            
        }
        
        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();

            var scrollView = StyleUtils.SimpleScrollView();
            
            //todo add gui
            scrollView.Add(_initializationView.CreateUI());

            
            root.Add(scrollView);
        }
    }
}