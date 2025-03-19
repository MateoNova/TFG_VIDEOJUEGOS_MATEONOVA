using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace GraphBasedGenerator
{
    /// <summary>
    /// Represents a node in the graph, extending the Node class.
    /// </summary>
    public class GraphNode : Node
    {
        #region Fields

        /// <summary>
        /// The unique identifier for the node.
        /// </summary>
        public string NodeId;

        /// <summary>
        /// The file path to the JSON file associated with the node.
        /// </summary>
        public string JsonFilePath = "";

        private Label _jsonFileLabel = new("None");

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the GraphNode class.
        /// </summary>
        /// <param name="position">The position of the node.</param>
        /// <param name="size">The size of the node.</param>
        public GraphNode(Vector2 position, Vector2 size)
        {
            NodeId = Guid.NewGuid().ToString();

            var rect = new Rect(position, size);
            SetPosition(rect);

            var inputPort = CreatePort(Direction.Input);
            inputContainer.Add(inputPort);

            var outputPort = CreatePort(Direction.Output);
            outputContainer.Add(outputPort);

            CreateJsonFileUI();
            RefreshExpandedState();
        }

        #endregion

        #region Port Creation

        /// <summary>
        /// Creates a port for the node.
        /// </summary>
        /// <param name="portDirection">The direction of the port (Input/Output).</param>
        /// <returns>The created port.</returns>
        private static Port CreatePort(Direction portDirection)
        {
            var port = Port.Create<Edge>(Orientation.Vertical, portDirection, Port.Capacity.Multi, typeof(object));
            return port;
        }

        #endregion

        #region JSON File UI

        /// <summary>
        /// Creates the UI for selecting a JSON file.
        /// </summary>
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

        #endregion

        #region Position

        /// <summary>
        /// Sets the position of the node.
        /// </summary>
        /// <param name="newPos">The new position of the node.</param>
        public sealed override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
        }

        #endregion

        #region JSON File Label

        /// <summary>
        /// Updates the label displaying the JSON file name.
        /// </summary>
        public void UpdateJsonFileLabel()
        {
            if (_jsonFileLabel == null)
            {
                Debug.LogError("JsonFileLabel is not initialized.");
                return;
            }
            
            var fileName = Path.GetFileName(JsonFilePath);
            _jsonFileLabel.text = fileName.Length > 5 ? fileName[..^5] : fileName;
        }

        #endregion
    }
}