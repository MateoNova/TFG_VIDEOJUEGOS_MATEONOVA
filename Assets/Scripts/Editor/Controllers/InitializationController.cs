using System;
using UnityEditor;

namespace Editor.Controllers
{
    public class InitializationController
    {
        public Action _onReload;
        
        public InitializationController(/*GeneratorSelection generatorSelection*/)
        {
            //_generatorSelection = generatorSelection;
        }
        internal void InitScene()
        {
            /*_generatorSelection.RetrieveOrInitializeCachedGenerationManager();
            _generatorSelection.FindAllGenerators();
            _generatorSelection.SelectGenerator(0);*/
        }

        internal void ReloadAll()
        {
            _onReload?.Invoke();
        }

        internal void ClearCachedData()
        {
            EditorPrefs.DeleteAll();
            //_generatorSelection.ClearCacheData();
        }
    }
}