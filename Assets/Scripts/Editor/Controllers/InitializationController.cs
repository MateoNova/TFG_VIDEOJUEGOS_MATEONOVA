using System;
using UnityEditor;

namespace Editor.Controllers
{
    public class InitializationController
    {
        public static Action _onReload;
        public static Action _onInitScene;
        public static Action _onClearCachedData;
        
        public InitializationController()
        {
            //_generatorSelection = generatorSelection;
        }
        internal void InitScene()
        {
            _onInitScene.Invoke();
            
        }

        internal void ReloadAll()
        {
            _onReload?.Invoke();
        }

        internal void ClearCachedData()
        {
            EditorPrefs.DeleteAll();
            _onClearCachedData?.Invoke();
            //_generatorSelection.ClearCacheData();
        }
    }
}