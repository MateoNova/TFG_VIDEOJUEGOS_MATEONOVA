using Editor.Controllers;
using UnityEngine.UIElements;

namespace Editor.Views
{
    public class InitializationView
    {
        private readonly InitializationController _initializationController = new();
        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();

            var foldout = new Foldout { text = "Initialization", value = true };

            var buttonContainer = StyleUtils.RowButtonContainer();
            var clearButton = StyleUtils.ButtonInRowContainer("Clear and Delete", _initializationController.ClearCachedData, true);
            var initButton = StyleUtils.ButtonInRowContainer("Initialize Scene", _initializationController.InitScene);
            var reloadAll = StyleUtils.ButtonInRowContainer("Reload", _initializationController.ReloadAll);

            buttonContainer.Add(clearButton);
            buttonContainer.Add(initButton);
            buttonContainer.Add(reloadAll);

            foldout.Add(buttonContainer);
            container.Add(foldout);

            return container;
        }
    }
}