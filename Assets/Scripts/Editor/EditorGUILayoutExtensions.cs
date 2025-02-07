using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class EditorGUILayoutExtensions
    {
        /// <summary>
        /// Creates a horizontal layout group and executes the provided content action within it.
        /// </summary>
        /// <param name="content">The action to execute within the horizontal layout group.</param>
        public static void Horizontal(Action content)
        {
            EditorGUILayout.BeginHorizontal();
            content();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a vertical layout group with an optional style and executes the provided content action within it.
        /// </summary>
        /// <param name="content">The action to execute within the vertical layout group.</param>
        /// <param name="style">The GUIStyle to apply to the vertical layout group. If null, GUIStyle.none is used.</param>
        public static void Vertical(Action content, GUIStyle style = null)
        {
            if (style == null) style = GUIStyle.none;

            EditorGUILayout.BeginVertical(style);
            content();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a section title with a bold label and some spacing before and after the title.
        /// </summary>
        /// <param name="title">The title text to display.</param>
        public static void DrawSectionTitle(string title)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(title, new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 });
            EditorGUILayout.Space();
        }
    }
}