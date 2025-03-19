using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphBasedGenerator
{
    /// <summary>
    /// Represents the editor window for the graph generator.
    /// </summary>
    public class GraphWindow : EditorWindow
    {
        /// <summary>
        /// Shows the Graph Window.
        /// </summary>
        //[MenuItem("Window/Graph Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<GraphWindow>("Graph Window");
            window.minSize = new Vector2(400, 300);

            var graphView = new GraphGeneratorView();
            graphView.name = "GraphGeneratorView";
            graphView.StretchToParentSize();
            window.rootVisualElement.Add(graphView);
        }

        /// <summary>
        /// Gets the GraphGeneratorView from the window.
        /// </summary>
        /// <returns>The GraphGeneratorView instance.</returns>
        public static GraphGeneratorView GetGraphGeneratorView()
        {
            var window = GetWindow<GraphWindow>("Graph Window"); 
            return window.rootVisualElement.Q<GraphGeneratorView>("GraphGeneratorView");
        }
    }
}