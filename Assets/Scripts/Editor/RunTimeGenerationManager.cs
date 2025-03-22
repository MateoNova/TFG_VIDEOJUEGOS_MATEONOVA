namespace Editor
{
    using UnityEngine;
    
    public class RuntimeGenerationManager : MonoBehaviour
    {
        private GeneratorSelection _generatorSelection;
        private GeneratorSettings _generatorSettings;
        private StyleManager _styleManager;
        private GenerationActions _generationActions;
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeOnLoad()
        {
            var manager = new GameObject("RuntimeGenerationManager").AddComponent<RuntimeGenerationManager>();
            manager.InitializeDependencies();
            manager.InitScene();
        }
    
        private void InitializeDependencies()
        {
            _generatorSelection = new GeneratorSelection();
            _generatorSettings = new GeneratorSettings(_generatorSelection);
            _styleManager = new StyleManager(_generatorSelection);
            _generationActions = new GenerationActions(_generatorSelection);
        }
    
        private void InitScene()
        {
            _generatorSelection.RetrieveOrInitializeCachedGenerationManager();
            _generatorSelection.FindAllGenerators();
            _generatorSelection.SelectGenerator(0);
        }
    
    }
}