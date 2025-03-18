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
            this.AddManipulator(new ContextualMenuManipulator(AddCustomContextMenu));
            RefreshExpandedState();
        }

        private Port CreatePort(Direction portDirection)
        {
            var port = Port.Create<Edge>(Orientation.Vertical, portDirection, Port.Capacity.Multi, typeof(object));
            return port;
        }

        private void AddCustomContextMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Connection", _ =>
            {
                var graphView = this.GetFirstAncestorOfType<GraphGeneratorView>();
                if (graphView == null)
                    return;

                if (GraphGeneratorView.PendingConnectionNode == null)
                {
                    GraphGeneratorView.PendingConnectionNode = this;
                    Debug.Log(
                        "Connection source set. Now right-click on the target node and select 'Create Connection' again.");
                }
                else if (GraphGeneratorView.PendingConnectionNode == this)
                {
                    GraphGeneratorView.PendingConnectionNode = null;
                    Debug.Log("Connection source cleared.");
                }
                else
                {
                    var sourceNode = GraphGeneratorView.PendingConnectionNode;
                    var sourcePort = sourceNode.outputContainer[0] as Port;
                    var targetPort = this.inputContainer[0] as Port;

                    var edge = new Edge
                    {
                        output = sourcePort,
                        input = targetPort
                    };
                    edge.UpdateEdgeControl();

                    graphView.AddElement(edge);
                    Debug.Log("Created connection between nodes.");
                    GraphGeneratorView.PendingConnectionNode = null;
                }
            });
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
                var fileName = Path.GetFileName(JsonFilePath);
                _jsonFileLabel.text = fileName[..^5];
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
    }
}