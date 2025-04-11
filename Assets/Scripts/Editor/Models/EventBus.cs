using System;

namespace Editor.Models
{
    public static class EventBus
    {
        public static event Action Reload;
        public static event Action InitScene;
        public static event Action ClearCachedData;
        public static event Action GeneratorChanged;
        public static event Action<bool> ToggleOpenGraphButton;
        public static event Action ToolOpened;

        public static void OnReload() => Reload?.Invoke();
        public static void OnInitScene() => InitScene?.Invoke();
        public static void OnClearCachedData() => ClearCachedData?.Invoke();
        public static void OnGeneratorChanged() => GeneratorChanged?.Invoke();
        public static void OnToggleOpenGraphButton(bool show) => ToggleOpenGraphButton?.Invoke(show);
        public static void OnToolOpened() => ToolOpened?.Invoke();
    }
}