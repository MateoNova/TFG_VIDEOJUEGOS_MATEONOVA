using Editor.Models;
using UnityEditor;

namespace Editor.Controllers
{
    /// <summary>
    /// Controller responsible for handling initialization, reloading, and clearing cached data in the editor.
    /// </summary>
    public class InitializationController
    {
        /// <summary>
        /// Initializes the scene by triggering the OnInitScene event on the EventBus.
        /// </summary>
        public void InitScene()
        {
            EventBus.OnInitScene();
            EventBus.OnReload();
        }

        /// <summary>
        /// Reloads all relevant data or components by triggering the OnReload event on the EventBus.
        /// </summary>
        public void ReloadAll()
        {
            EventBus.OnReload();
        }

        /// <summary>
        /// Clears all cached data by deleting all editor preferences and triggering the OnClearCachedData event on the EventBus.
        /// </summary>
        public void ClearCachedData()
        {
            EditorPrefs.DeleteAll();
            EventBus.OnClearCachedData();
        }
    }
}