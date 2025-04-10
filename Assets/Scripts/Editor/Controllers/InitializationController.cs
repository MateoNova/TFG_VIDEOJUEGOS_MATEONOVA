using Editor.Models;
using UnityEditor;

namespace Editor.Controllers
{
    public class InitializationController
    {
        public void InitScene()
        {
            EventBus.OnInitScene();
        }

        public void ReloadAll()
        {
            // Se puede agregar más lógica si es necesario
            EventBus.OnReload();
        }

        public void ClearCachedData()
        {
            // Limpieza de EditorPrefs y demás
            EditorPrefs.DeleteAll();
            EventBus.OnClearCachedData();
        }
    }
}