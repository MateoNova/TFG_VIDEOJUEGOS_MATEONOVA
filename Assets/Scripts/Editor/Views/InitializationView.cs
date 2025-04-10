using Editor.Controllers;
using UnityEngine.UIElements;

namespace Editor.Views
{
    public class InitializationView
    {
        private readonly InitializationController _controller = new InitializationController();

        public VisualElement CreateUI()
        {
            VisualElement container = StyleUtils.SimpleContainer();
            Foldout foldout = new Foldout { text = "Initialization", value = true };
            foldout.Add(CreateButtonContainer());
            container.Add(foldout);
            return container;
        }

        private VisualElement CreateButtonContainer()
        {
            VisualElement buttonContainer = StyleUtils.RowButtonContainer();
            buttonContainer.Add(CreateButton("Clear and Delete", _controller.ClearCachedData, true));
            buttonContainer.Add(CreateButton("Initialize Scene", _controller.InitScene));
            buttonContainer.Add(CreateButton("Reload", _controller.ReloadAll));
            return buttonContainer;
        }

        private static Button CreateButton(string text, System.Action onClick, bool isPrimary = false)
        {
            return StyleUtils.ButtonInRowContainer(text, onClick, isPrimary);
        }
    }
}