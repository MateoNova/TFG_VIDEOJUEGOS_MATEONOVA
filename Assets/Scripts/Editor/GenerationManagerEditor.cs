using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class GenerationManagerWindow : EditorWindow
    {
        private InitializationManager _initializationManager;
        private GeneratorSelection _generatorSelection;
        private GeneratorSettings _generatorSettings;
        private StyleManager _styleManager;
        private GenerationActions _generationActions;

        [MenuItem("Window/Generation Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<GenerationManagerWindow>("Generation Manager");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            InitializeDependencies();
            _initializationManager.InitScene();
        }

        /// <summary>
        /// Inicializa las dependencias de la ventana.
        /// </summary>
        private void InitializeDependencies()
        {
            _generatorSelection = new GeneratorSelection();
            _initializationManager = new InitializationManager(_generatorSelection);
            _generatorSettings = new GeneratorSettings(_generatorSelection);
            _styleManager = new StyleManager(_generatorSelection);
            _generationActions = new GenerationActions(_generatorSelection);
        }

        /// <summary>
        /// Crea la interfaz de usuario utilizando UI Toolkit.
        /// </summary>
        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();

            // Opcional: Cargar hoja de estilos USS si la tienes
            // root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Path/To/YourStyles.uss"));
            
            // Scroll para el contenido
            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;

            // Se añaden los VisualElements generados por cada manager
            scrollView.Add(_initializationManager.CreateUI());
            scrollView.Add(_generatorSelection.CreateUI());
            scrollView.Add(_generatorSettings.CreateUI());
            scrollView.Add(_styleManager.CreateUI());
            scrollView.Add(_generationActions.CreateUI());

            root.Add(scrollView);
        }
    }
}
