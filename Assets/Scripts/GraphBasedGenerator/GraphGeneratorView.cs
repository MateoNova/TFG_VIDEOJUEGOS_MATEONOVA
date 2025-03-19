using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace GraphBasedGenerator
{
    public class GraphGeneratorView : GraphView
    {
        public static GraphNode PendingConnectionNode;

        public GraphGeneratorView()
        {
            AddManipulators();
            AddBackground();
            AddStyles();
            AddCopyPasteHandlers();
        }

        private void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
        }

        private void AddStyles()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Assets/Editor/GraphBackground.uss");
            styleSheets.Add(styleSheet);
        }

        private void AddBackground()
        {
            var background = new GridBackground();
            Insert(0, background);
            background.StretchToParentSize();
        }

        private new void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is not GraphGeneratorView) return;

            var localMousePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("Create Node", _ => CreateNode(localMousePosition));
        }

        private void CreateNode(Vector2 position)
        {
            var node = new GraphNode(position, new Vector2(200, 200));
            AddElement(node);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node)
                    compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

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

        private void CopySelection()
        {
            var selectedNodes = selection.OfType<GraphNode>().ToList();
            if (selectedNodes.Count == 0) return;

            //copy the position and json file path of each node
            var data = selectedNodes.Select(node => new NodeData
            {
                Position = node.GetPosition().position,
                JsonFilePath = node.JsonFilePath
            }).ToArray();
            
            //convert the data to json and copy it to the clipboard
            var json = string.Join("\n", data.Select(JsonUtility.ToJson));
            EditorGUIUtility.systemCopyBuffer = json;
        }

        private void PasteSelection()
        {
            var data = EditorGUIUtility.systemCopyBuffer.Split('\n');
            foreach (var nodeData in data)
            {
                var deserializedNode = JsonUtility.FromJson<NodeData>(nodeData);
                // Crea el nodo
                var newNode = new GraphNode(deserializedNode.Position, new Vector2(200, 200));
                // Asigna el JsonFilePath copiado
                newNode.JsonFilePath = deserializedNode.JsonFilePath;
                // Opcional: si el nodo tiene un label que muestra el nombre del fichero, actualizarlo
                newNode.UpdateJsonFileLabel(); // si implementas un método para eso
                AddElement(newNode);
            }
        }


        [System.Serializable]
        private class NodeData
        {
            public Vector2 Position;
            public string JsonFilePath;
        }
    }
}