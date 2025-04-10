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
            var foldout = CreateInitializationFoldout();
            container.Add(foldout);
            return container;
        }

        private Foldout CreateInitializationFoldout()
        {
            var foldout = new Foldout { text = "Initialization", value = true };
            var buttonContainer = CreateButtonContainer();
            foldout.Add(buttonContainer);
            return foldout;
        }

        private VisualElement CreateButtonContainer()
        {
            var buttonContainer = StyleUtils.RowButtonContainer();

            buttonContainer.Add(CreateButton("Clear and Delete", _initializationController.ClearCachedData, true));
            buttonContainer.Add(CreateButton("Initialize Scene", _initializationController.InitScene));
            buttonContainer.Add(CreateButton("Reload", _initializationController.ReloadAll));

            return buttonContainer;
        }

        private static Button CreateButton(string text, System.Action onClick, bool isDestructive = false)
        {
            return StyleUtils.ButtonInRowContainer(text, onClick, isDestructive);
        }
    }
}