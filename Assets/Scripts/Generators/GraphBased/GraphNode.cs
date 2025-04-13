using System;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Generators.GraphBased
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

            //add style to the title
            title = "Unnasigned node";
            titleContainer.AddToClassList("graph-node-title");

            AddToClassList("graph-node");

            var rect = new Rect(position, size);
            SetPosition(rect);

            var inputPort = CreatePort(Direction.Input, "IN");
            inputContainer.Add(inputPort);

            var outputPort = CreatePort(Direction.Output, "OUT");
            outputContainer.Add(outputPort);

            CreateJsonFileUI();
            RefreshExpandedState();
        }

        public sealed override string title
        {
            get { return base.title; }
            set { base.title = value; }
        }

        #endregion

        #region Port Creation

        /// <summary>
        /// Creates a port for the node.
        /// </summary>
        /// <param name="portDirection">The direction of the port (Input/Output).</param>
        /// <returns>The created port.</returns>
        private static Port CreatePort(Direction portDirection, string name)
        {
            var port = Port.Create<Edge>(Orientation.Vertical, portDirection, Port.Capacity.Multi, typeof(object));
            port.portName = name;
            return port;
        }

        #endregion

        #region JSON File UI

        /// <summary>
        /// Creates the UI for selecting a JSON file.
        /// </summary>
        private void CreateJsonFileUI()
        {
            var container = new VisualElement();
            container.AddToClassList("json-container");

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

            selectButton.AddToClassList("select-dungeon-button");

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
            var fileName = Path.GetFileName(JsonFilePath);
            title = fileName.Length > 5 ? fileName[..^5] : fileName;
        }

        #endregion
    }
}