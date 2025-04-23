using UnityEditor;
using EventBus = Models.Editor.EventBus;

namespace Controllers.Editor
{
    /// <summary>
    /// Controller responsible for handling initialization, reloading, and clearing cached data in the editor.
    /// </summary>
    public static class InitializationController
    {
        /// <summary>
        /// Initializes the scene by triggering the OnInitScene event on the EventBus.
        /// </summary>
        public static void InitScene()
        {
            EventBus.OnInitScene();
            EventBus.OnReload();
        }

        /// <summary>
        /// Reloads all relevant data or components by triggering the OnReload event on the EventBus.
        /// </summary>
        public static void ReloadAll()
        {
            EventBus.OnReload();
        }

        /// <summary>
        /// Clears all cached data by deleting all editor preferences and triggering the OnClearCachedData event on the EventBus.
        /// </summary>
        public static void ClearCachedData()
        {
            EditorPrefs.DeleteAll();
            EventBus.OnClearCachedData();
        }
    }
}