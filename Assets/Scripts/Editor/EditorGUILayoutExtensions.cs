using System;
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
        /// Creates a horizontal group and executes the provided action within it.
        /// </summary>
        /// <param name="content">Action to execute within the horizontal group.</param>
        public static void Horizontal(Action content)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                content?.Invoke();
            }
        }

        /// <summary>
        /// Creates a vertical group with an optional style and executes the provided action within it.
        /// </summary>
        /// <param name="content">Action to execute within the vertical group.</param>
        /// <param name="style">Style to apply to the vertical group. If null, <see cref="GUIStyle.none"/> is used.</param>
        /// <param name="options">Layout options for the vertical group.</param>
        public static void Vertical(Action content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(style ?? GUIStyle.none, options))
            {
                content?.Invoke();
            }
        }

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