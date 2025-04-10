using Editor.Controllers;
using UnityEngine.UIElements;

namespace Editor.Views
{
    public class ActionsView
    {
        private bool _showGenerationActions = true;
        private ActionsController _actionsController = new ActionsController();

        public VisualElement CreateUI()
        {
            VisualElement container = StyleUtils.SimpleContainer();
            Foldout actionsFoldout = new Foldout { text = "Generation Actions", value = _showGenerationActions };
            actionsFoldout.RegisterValueChangedCallback(evt => _showGenerationActions = evt.newValue);
            container.Add(actionsFoldout);

            if (_showGenerationActions)
            {
                AddClearToggle(actionsFoldout);
                VisualElement buttonsContainer = AddActionsButtons();
                actionsFoldout.Add(buttonsContainer);
            }

            return container;
        }

        private VisualElement AddActionsButtons()
        {
            VisualElement buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Column;
            buttonsContainer.style.marginTop = 5;

            Button generateButton = new Button(_actionsController.Generate) { text = "Generate Dungeon" };
            Button clearButton = new Button(_actionsController.ClearDungeon) { text = "Clear Dungeon" };
            Button saveButton = new Button(_actionsController.SaveDungeon) { text = "Save Dungeon" };
            Button loadButton = new Button(_actionsController.LoadDungeon) { text = "Load Dungeon" };

            buttonsContainer.Add(generateButton);
            buttonsContainer.Add(clearButton);
            buttonsContainer.Add(saveButton);
            buttonsContainer.Add(loadButton);

            return buttonsContainer;
        }

        private void AddClearToggle(Foldout actionsFoldout)
        {
            Toggle clearToggle = StyleUtils.SimpleToggle("Clear Dungeon", _actionsController.ClearDungeonToggle,
                "Clear the dungeon before generating.");
            clearToggle.RegisterValueChangedCallback(evt => _actionsController.SetClearDungeon(evt.newValue));
            actionsFoldout.Add(clearToggle);
        }
    }
}