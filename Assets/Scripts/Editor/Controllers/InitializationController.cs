using System;
using UnityEditor;

namespace Editor.Controllers
{
    /// <summary>
    /// Controller responsible for handling initialization-related actions.
    /// </summary>
    public class InitializationController
    {
        /// <summary>
        /// Event triggered when a reload action is performed.
        /// </summary>
        public static event Action OnReload;

        /// <summary>
        /// Event triggered when a scene initialization action is performed.
        /// </summary>
        public static event Action OnInitScene;

        /// <summary>
        /// Event triggered when cached data is cleared.
        /// </summary>
        public static event Action OnClearCachedData;

        /// <summary>
        /// Invokes the <see cref="OnInitScene"/> event to initialize the scene.
        /// </summary>
        internal void InitScene()
        {
            OnInitScene?.Invoke();
        }

        /// <summary>
        /// Invokes the <see cref="OnReload"/> event to reload all relevant data or states.
        /// </summary>
        internal void ReloadAll()
        {
            OnReload?.Invoke();
        }

        /// <summary>
        /// Clears all cached data and invokes the <see cref="OnClearCachedData"/> event.
        /// </summary>
        internal void ClearCachedData()
        {
            EditorPrefs.DeleteAll();
            OnClearCachedData?.Invoke();
        }
    }
}