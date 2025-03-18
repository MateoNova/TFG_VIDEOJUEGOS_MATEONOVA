using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphBasedGenerator
{
    public class GraphWindow : EditorWindow
    {
        [MenuItem("Window/Graph Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<GraphWindow>("Graph Window");
            window.minSize = new Vector2(400, 300);

            var graphView = new GraphGeneratorView();

            graphView.StretchToParentSize();
            window.rootVisualElement.Add(graphView);
        }

        private void OnDestroy()
        {
            if (EditorUtility.DisplayDialog("Confirm Close", "Are you sure you want to close the Graph Window?", "Yes",
                    "No")) return;
            // Cancel the close operation
            var window = GetWindow<GraphWindow>("Graph Window");
            window.Show();
        }
        
        public static GraphGeneratorView getGraphGeneratorView() {
            var window = GetWindow<GraphWindow>("Graph Window");
            return window.rootVisualElement.Q<GraphGeneratorView>();
        }
    }
}