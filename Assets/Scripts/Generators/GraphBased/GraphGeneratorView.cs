using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Generators.GraphBased
{
    /// <summary>
    /// Represents the view for the graph generator, extending the GraphView class.
    /// </summary>
    public class GraphGeneratorView : GraphView
    {
        /// <summary>
        /// Indicates whether grid snapping is enabled.
        /// </summary>
        private bool _gridSnappingEnabled;

        /// <summary>
        /// The size of the grid for snapping.
        /// </summary>
        private const float GridSize = 25f;

        /// <summary>
        /// Initializes a new instance of the GraphGeneratorView class.
        /// </summary>
        public GraphGeneratorView()
        {
            AddManipulators();
            AddBackground();
            AddStyles();
            AddCopyPasteHandlers();

            // Callback to snap selected nodes to the grid when the mouse is released
            RegisterCallback<MouseUpEvent>(_ =>
            {
                if (_gridSnappingEnabled)
                {
                    SnapSelectedNodesToGrid();
                }
            });
        }

        /// <summary>
        /// Snaps the selected nodes to the grid.
        /// </summary>
        private void SnapSelectedNodesToGrid()
        {
            foreach (var element in selection)
            {
                if (element is not GraphNode node) continue;

                var pos = node.GetPosition();
                var snappedX = Mathf.Round(pos.x / GridSize) * GridSize;
                var snappedY = Mathf.Round(pos.y / GridSize) * GridSize;
                node.SetPosition(new Rect(snappedX, snappedY, pos.width, pos.height));
            }
        }

        #region Manipulators

        /// <summary>
        /// Adds manipulators to the graph view.
        /// </summary>
        private void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        #endregion

        #region Styles

        /// <summary>
        /// Adds styles to the graph view.
        /// </summary>
        private void AddStyles()
        {
            var backgroundStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Assets/Editor/GraphBackground.uss");
            styleSheets.Add(backgroundStyle);

            var nodeStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Assets/Editor/GraphNodeStyle.uss");
            styleSheets.Add(nodeStyle);
        }

        #endregion

        #region Background

        /// <summary>
        /// Adds a background to the graph view.
        /// </summary>
        private void AddBackground()
        {
            var background = new GridBackground();
            Insert(0, background);
            background.StretchToParentSize();
        }

        #endregion

        #region Context Menu

        /// <summary>
        /// Builds the contextual menu for the graph view.
        /// </summary>
        /// <param name="evt">The event holding the menu to populate.</param>
        private new void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is not GraphGeneratorView) return;
            var localMousePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("Create Node", _ => CreateNode(localMousePosition));
            evt.menu.AppendAction("Toggle Grid Snapping", _ => { _gridSnappingEnabled = !_gridSnappingEnabled; });
            evt.menu.AppendAction("Save Graph", _ => SaveGraph());
            evt.menu.AppendAction("Load Graph", _ => LoadGraph());
        }

        #endregion

        #region Node Creation

        /// <summary>
        /// Creates a new node at the specified position.
        /// </summary>
        /// <param name="position">The position to create the node at.</param>
        private void CreateNode(Vector2 position)
        {
            var node = new GraphNode(position, new Vector2(200, 200));
            AddElement(node);
        }

        #endregion

        #region Port Compatibility

        /// <summary>
        /// Gets the compatible ports for a given start port.
        /// </summary>
        /// <param name="startPort">The start port.</param>
        /// <param name="nodeAdapter">The node adapter.</param>
        /// <returns>A list of compatible ports.</returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                    compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        #endregion

        #region Copy-Paste Handlers

        /// <summary>
        /// Adds copy-paste handlers to the graph view.
        /// </summary>
        private void AddCopyPasteHandlers()
        {
            RegisterCallback<KeyDownEvent>(evt =>
            {
                switch (evt.ctrlKey)
                {
                    case true when evt.keyCode == KeyCode.C:
                        CopySelection();
                        break;
                    case true when evt.keyCode == KeyCode.V:
                        PasteSelection();
                        break;
                }
            });
        }

        /// <summary>
        /// Copies the selected nodes to the clipboard.
        /// </summary>
        private void CopySelection()
        {
            var selectedNodes = selection.OfType<GraphNode>().ToList();
            if (selectedNodes.Count == 0) return;

            var data = selectedNodes.Select(node => new NodeData
            {
                position = node.GetPosition().position,
                jsonFilePath = node.JsonFilePath
            }).ToArray();

            var json = string.Join("\n", data.Select(JsonUtility.ToJson));
            EditorGUIUtility.systemCopyBuffer = json;
        }

        /// <summary>
        /// Pastes the nodes from the clipboard.
        /// </summary>
        private void PasteSelection()
        {
            var data = EditorGUIUtility.systemCopyBuffer.Split('\n');
            foreach (var nodeData in data)
            {
                var deserializedNode = JsonUtility.FromJson<NodeData>(nodeData);
                var newNode = new GraphNode(deserializedNode.position, new Vector2(200, 200))
                {
                    JsonFilePath = deserializedNode.jsonFilePath
                };
                newNode.UpdateJsonFileLabel();
                AddElement(newNode);
            }
        }

        #endregion

        #region Graph Save/Load

        /// <summary>
        /// Saves the current graph to a file.
        /// </summary>
        private void SaveGraph()
        {
            var graphData = new GraphData
            {
                nodes = nodes.OfType<GraphNode>().Select(node => new NodeData
                {
                    nodeId = node.NodeId,
                    position = node.GetPosition().position,
                    jsonFilePath = node.JsonFilePath
                }).ToList(),
                edges = edges.Select(edge => new EdgeData
                {
                    sourceId = (edge.output.node as GraphNode)?.NodeId,
                    targetId = (edge.input.node as GraphNode)?.NodeId
                }).ToList()
            };

            var json = JsonUtility.ToJson(graphData, true);
            var path = EditorUtility.SaveFilePanel("Save graph", "", "GraphData.json", "json");

            if (string.IsNullOrEmpty(path)) return;
            System.IO.File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads a graph from a file.
        /// </summary>
        private void LoadGraph()
        {
            var path = EditorUtility.OpenFilePanel("Load Graph", "", "json");
            if (string.IsNullOrEmpty(path))
                return;

            var json = System.IO.File.ReadAllText(path);
            var graphData = JsonUtility.FromJson<GraphData>(json);

            ClearGraph();

            var nodeDict = new Dictionary<string, GraphNode>();

            foreach (var n in graphData.nodes)
            {
                var node = new GraphNode(n.position, new Vector2(200, 200))
                {
                    JsonFilePath = n.jsonFilePath
                };
                node.UpdateJsonFileLabel();
                node.NodeId = n.nodeId;
                AddElement(node);
                nodeDict.Add(n.nodeId, node);
            }

            foreach (var edge in from e in graphData.edges
                     where nodeDict.ContainsKey(e.sourceId) && nodeDict.ContainsKey(e.targetId)
                     let source = nodeDict[e.sourceId]
                     let target = nodeDict[e.targetId]
                     let outputPort = source.outputContainer[0] as Port
                     let inputPort = target.inputContainer[0] as Port
                     select outputPort?.ConnectTo(inputPort))
            {
                AddElement(edge);
            }
        }

        /// <summary>
        /// Clears the current graph.
        /// </summary>
        private void ClearGraph()
        {
            foreach (var element in graphElements.ToList())
            {
                RemoveElement(element);
            }
        }

        #endregion

        #region Data Classes

        /// <summary>
        /// Represents the data for the graph.
        /// </summary>
        [System.Serializable]
        public class GraphData
        {
            public List<NodeData> nodes;
            public List<EdgeData> edges;
        }

        /// <summary>
        /// Represents the data for a node.
        /// </summary>
        [System.Serializable]
        public class NodeData
        {
            public string nodeId;
            public Vector2 position;
            public string jsonFilePath;
        }

        /// <summary>
        /// Represents the data for an edge.
        /// </summary>
        [System.Serializable]
        public class EdgeData
        {
            public string sourceId;
            public string targetId;
        }

        #endregion
    }
}