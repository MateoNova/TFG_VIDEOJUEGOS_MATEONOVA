using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace GraphBasedGenerator
{
    public class GraphNode : Node
    {
        public string JsonFilePath = "";
        private Label _jsonFileLabel;

        public GraphNode(Vector2 position, Vector2 size)
        {
            var rect = new Rect(position, size);
            SetPosition(rect);

            var inputPort = CreatePort(Direction.Input);
            inputContainer.Add(inputPort);

            var outputPort = CreatePort(Direction.Output);
            outputContainer.Add(outputPort);

            CreateJsonFileUI();
            RefreshExpandedState();
        }

        private Port CreatePort(Direction portDirection)
        {
            var port = Port.Create<Edge>(Orientation.Vertical, portDirection, Port.Capacity.Multi, typeof(object));
            return port;
        }

        private void CreateJsonFileUI()
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = 5
                }
            };

            _jsonFileLabel = new Label("None");
            container.Add(_jsonFileLabel);

            var selectButton = new Button(() =>
            {
                var path = EditorUtility.OpenFilePanel("JSON selection", "", "json");
                if (string.IsNullOrEmpty(path)) return;
                JsonFilePath = path;
                UpdateJsonFileLabel();
            })
            {
                text = "Select dungeon"
            };

            container.Add(selectButton);
            extensionContainer.Add(container);
        }

        public sealed override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
        }

        public void UpdateJsonFileLabel()
        {
            var fileName = Path.GetFileName(JsonFilePath);
            _jsonFileLabel.text = fileName[..^5];
        }
    }
}