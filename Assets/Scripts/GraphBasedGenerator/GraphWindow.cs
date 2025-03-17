using UnityEditor;
using UnityEngine;

namespace GraphBasedGenerator
{
    public class GraphWindow : EditorWindow
    {
        private static bool _isOpen;

        [MenuItem("Window/Graph Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<GraphWindow>("Graph Window");
            window.minSize = new Vector2(400, 300);
            _isOpen = true;
        }

        private void OnGUI()
        {
            GUILayout.Label("Graph Window", EditorStyles.boldLabel);
        }

        private void OnDestroy()
        {
            if (EditorUtility.DisplayDialog("Confirm Close", "Are you sure you want to close the Graph Window?", "Yes",
                    "No"))
            {
                _isOpen = false;
            }
            else
            {
                // Cancel the close operation
                var window = GetWindow<GraphWindow>("Graph Window");
                window.Show();
            }
        }

        public static bool IsOpen => _isOpen;
    }
}