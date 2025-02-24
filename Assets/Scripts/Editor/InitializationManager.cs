using UnityEditor;
using UnityEngine.UIElements;

namespace Editor
{
    /// <summary>
    /// Manages the initialization process for the generation system.
    /// </summary>
    public class InitializationManager
    {
        private readonly GeneratorSelection _generatorSelection;
        private Foldout _foldout;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationManager"/> class.
        /// </summary>
        /// <param name="generatorSelection">The generator selection instance.</param>
        public InitializationManager(GeneratorSelection generatorSelection)
        {
            _generatorSelection = generatorSelection;
        }

        /// <summary>
        /// Creates the UI for the initialization manager.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements.</returns>
        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();

            _foldout = new Foldout { text = "Initialization", value = true };

            var buttonContainer = StyleUtils.RowButtonContainer();
            var clearButton = StyleUtils.ButtonInRowContainer("Clear and Delete", ClearCachedData, true);
            var initButton = StyleUtils.ButtonInRowContainer("Initialize Scene", InitScene);

            buttonContainer.Add(clearButton);
            buttonContainer.Add(initButton);

            _foldout.Add(buttonContainer);
            container.Add(_foldout);

            return container;
        }

        /// <summary>
        /// Clears cached data by deleting all editor preferences and clearing the generator selection cache.
        /// </summary>
        private void ClearCachedData()
        {
            EditorPrefs.DeleteAll();
            _generatorSelection.ClearCacheData();
        }

        /// <summary>
        /// Initializes the scene by retrieving or initializing the cached generation manager,
        /// finding all generators, and selecting the first generator.
        /// </summary>
        public void InitScene()
        {
            _generatorSelection.RetrieveOrInitializeCachedGenerationManager();
            _generatorSelection.FindAllGenerators();
            _generatorSelection.SelectGenerator(0);
        }
    }
}