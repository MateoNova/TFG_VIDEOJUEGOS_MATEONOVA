using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphBasedGenerator
{
    public class GraphGeneratorView : GraphView
    {
        public GraphGeneratorView()
        {
            AddManipulators();
            AddBackground();
            AddStyles();
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

            var localMousePosition =
                this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);

            evt.menu.AppendAction("Create Node", _ => CreateNode(localMousePosition));
        }

        private void CreateNode(Vector2 position)
        {
            var node = new GraphNode(position, new Vector2(200, 200));
            AddElement(node);
        }
        
        
        
    }
}