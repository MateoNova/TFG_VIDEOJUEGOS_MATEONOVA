using System;

namespace Models.Editor
{
    /// <summary>
    /// A static class that acts as an event bus for managing and broadcasting events across the application.
    /// </summary>
    public static class EventBus
    {
        # region Events to subscribe to

        /// <summary>
        /// Event triggered when a reload action is performed.
        /// </summary>
        public static event Action Reload;

        /// <summary>
        /// Event triggered when the scene is initialized.
        /// </summary>
        public static event Action InitScene;

        /// <summary>
        /// Event triggered to clear cached data.
        /// </summary>
        public static event Action ClearCachedData;

        /// <summary>
        /// Event triggered when the generator is changed.
        /// </summary>
        public static event Action GeneratorChanged;

        /// <summary>
        /// Event triggered to toggle the visibility of the "Open Graph" button.
        /// </summary>
        public static event Action<bool> ToggleOpenGraphButton;

        /// <summary>
        /// Event triggered when a tool is opened.
        /// </summary>
        public static event Action ToolOpened;

        # endregion

        # region Event Invokers

        /// <summary>
        /// Invokes the Reload event.
        /// </summary>
        public static void OnReload() => Reload?.Invoke();

        /// <summary>
        /// Invokes the InitScene event.
        /// </summary>
        public static void OnInitScene() => InitScene?.Invoke();

        /// <summary>
        /// Invokes the ClearCachedData event.
        /// </summary>
        public static void OnClearCachedData() => ClearCachedData?.Invoke();

        /// <summary>
        /// Invokes the GeneratorChanged event.
        /// </summary>
        public static void OnGeneratorChanged() => GeneratorChanged?.Invoke();

        /// <summary>
        /// Invokes the ToggleOpenGraphButton event with the specified visibility state.
        /// </summary>
        /// <param name="show">A boolean indicating whether to show or hide the button.</param>
        public static void OnToggleOpenGraphButton(bool show) => ToggleOpenGraphButton?.Invoke(show);

        /// <summary>
        /// Invokes the ToolOpened event.
        /// </summary>
        public static void OnToolOpened() => ToolOpened?.Invoke();

        # endregion
    }
}