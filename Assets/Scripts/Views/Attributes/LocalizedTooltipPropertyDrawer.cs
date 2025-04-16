#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Views.Attributes
{
    /// <summary>
    /// Custom property drawer for the LocalizedTooltipAttribute.
    /// This drawer adds localized tooltips to properties in the Unity Inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(LocalizedTooltipAttribute))]
    public class LocalizedTooltipPropertyDrawer : PropertyDrawer
    {
        // Cache for localized strings to avoid repeated lookups.
        private static readonly Dictionary<string, string> LocalizedStringCache = new();

        // Flag to ensure localization is initialized only once.
        private static bool _isInitialized;

        // Fixed width for the label in the property field.
        private const float FixedLabelWidth = 150f;

        // Static constructor to subscribe to locale change events.
        static LocalizedTooltipPropertyDrawer()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        /// <summary>
        /// Clears the localized string cache when the locale changes.
        /// </summary>
        /// <param name="locale">The new selected locale.</param>
        private static void OnLocaleChanged(UnityEngine.Localization.Locale locale)
        {
            LocalizedStringCache.Clear();
        }

        /// <summary>
        /// Draws the property field with a localized tooltip in the Unity Inspector.
        /// </summary>
        /// <param name="position">The position of the property field.</param>
        /// <param name="property">The serialized property being drawn.</param>
        /// <param name="label">The label of the property field.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Adjust the position slightly to align the field properly.
            position.x -= 2;

            // Ensure localization is initialized.
            if (!_isInitialized)
            {
                LocalizationSettings.InitializationOperation.WaitForCompletion();

                if (!LocalizationSettings.SelectedLocale)
                {
                    //Debug.LogWarning("SelectedLocale is null. Setting to the first available locale.");
                    LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                }

                _isInitialized = true;
            }

            var localizedTooltip = (LocalizedTooltipAttribute)attribute;

            // Build the cache key for the localized string.
            var cacheKey = $"{localizedTooltip.TableName}:{localizedTooltip.Key}";

            // Retrieve or cache the localized string.
            if (!LocalizedStringCache.TryGetValue(cacheKey, out var localizedText))
            {
                localizedText = LocalizationSettings.StringDatabase.GetLocalizedString(
                    localizedTooltip.TableName, localizedTooltip.Key);
                LocalizedStringCache[cacheKey] = localizedText;
            }

            // Save the original label width and set a fixed width for the label.
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = FixedLabelWidth;

            // Create a GUIContent with the label text and the localized tooltip.
            var content = new GUIContent(label.text, localizedText);

            // Draw the property field with the adjusted position and content.
            EditorGUI.PropertyField(position, property, content, true);

            // Restore the original label width.
            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        /// <summary>
        /// Gets the height of the property field.
        /// </summary>
        /// <param name="property">The serialized property.</param>
        /// <param name="label">The label of the property field.</param>
        /// <returns>The height of the property field.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
#endif