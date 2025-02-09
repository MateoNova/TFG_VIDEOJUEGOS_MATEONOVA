using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Extensions to facilitate the creation of layouts in the Editor.
    /// </summary>
    public static class EditorGUILayoutExtensions
    {
        /// <summary>
        /// Style used to draw section titles.
        /// </summary>
        private static readonly GUIStyle BoldTitleStyle = new(EditorStyles.boldLabel)
        {
            fontSize = 14
        };

        /// <summary>
        /// Draws a section title with a bold label and adds spaces before and after.
        /// </summary>
        /// <param name="title">Text of the title to display.</param>
        public static void DrawSectionTitle(string title)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(title, BoldTitleStyle);
            EditorGUILayout.Space();
        }
    }
}