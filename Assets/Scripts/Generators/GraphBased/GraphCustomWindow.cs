using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Generators.GraphBased
{
    /// <summary>
    /// Represents the editor window for the graph generator.
    /// </summary>
    public class GraphCustomWindow : EditorWindow
    {
        /// <summary>
        /// Shows the Graph Window.
        /// </summary>
        //[MenuItem("Window/Graph Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<GraphCustomWindow>("Graph Window");
            window.minSize = new Vector2(400, 300);

            var graphView = new GraphGeneratorView
            {
                name = "GraphGeneratorView"
            };
            graphView.StretchToParentSize();
            window.rootVisualElement.Add(graphView);
        }

        /// <summary>
        /// Gets the GraphGeneratorView from the window.
        /// </summary>
        /// <returns>The GraphGeneratorView instance.</returns>
        public static GraphGeneratorView GetGraphGeneratorView()
        {
            var window = GetWindow<GraphCustomWindow>("Graph Window"); 
            return window.rootVisualElement.Q<GraphGeneratorView>("GraphGeneratorView");
        }
    }
}