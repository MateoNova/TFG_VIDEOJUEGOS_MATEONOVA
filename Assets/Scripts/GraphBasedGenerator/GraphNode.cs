using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace GraphBasedGenerator
{
    public class GraphNode : Node
    {
        private string _jsonFilePath = "";
        private Label _jsonFileLabel;

        public GraphNode(Vector2 position, Vector2 size)
        {
            var rect = new Rect(position, size);
            SetPosition(rect);
            CreateJsonFileUI();
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

            // Botón para abrir el selector de archivos JSON
            var selectButton = new Button(() =>
            {
                var path = EditorUtility.OpenFilePanel("JSON selection", "", "json");
                if (string.IsNullOrEmpty(path)) return;
                _jsonFilePath = path;
                var fileName = Path.GetFileName(_jsonFilePath);
                //delete .json extension
                _jsonFileLabel.text = fileName[..^5];
            })
            {
                text = "Select dungeon"
            };

            container.Add(selectButton);

            extensionContainer.Add(container);
            RefreshExpandedState();
        }

        public sealed override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
        }
    }
}