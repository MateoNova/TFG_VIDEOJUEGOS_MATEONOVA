using UnityEditor;
using UnityEngine.UIElements;

namespace Editor
{

    public class InitializationManager
    {
        private readonly GeneratorSelection _generatorSelection;
        private Foldout _foldout;
        
        public InitializationManager(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }
        
        public VisualElement CreateUI()
        {
            var container = Utils.CreateContainer();

            _foldout = new Foldout { text = "Initialization", value = true };

            var buttonContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    marginTop = 5
                }
            };

            var clearButton = new Button(ClearCachedData)
            {
                text = "Clear and Delete",
                style =
                {
                    height = 30,
                    marginRight = 5
                }
            };

            var initButton = new Button(InitScene)
            {
                text = "Initialize Scene",
                style =
                {
                    height = 30
                }
            };

            buttonContainer.Add(clearButton);
            buttonContainer.Add(initButton);

            _foldout.Add(buttonContainer);
            container.Add(_foldout);

            return container;
        }
        
        private void ClearCachedData()
        {
            EditorPrefs.DeleteAll();
            _generatorSelection.ClearCacheData();
        }
        
        public void InitScene()
        {
            //todo when everithing workds do this
            _generatorSelection.RetrieveOrInitializeCachedGenerationManager();
            _generatorSelection.FindAllGenerators();
            _generatorSelection.SelectGenerator(0);
        }
    }
}
