#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Views.Attributes
{
    [CustomPropertyDrawer(typeof(LocalizedTooltipAttribute))]
    public class LocalizedTooltipPropertyDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, string> LocalizedStringCache = new();
        private static bool _isInitialized;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Ensure the localization system is initialized only once
            if (!_isInitialized)
            {
                LocalizationSettings.InitializationOperation.WaitForCompletion();

                if (LocalizationSettings.SelectedLocale == null)
                {
                    Debug.LogWarning("SelectedLocale is null. Setting to the first available locale.");
                    LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                }

                _isInitialized = true;
            }

            // Retrieve the custom attribute
            var localizedTooltip = (LocalizedTooltipAttribute)attribute;

            // Check if the localized string is already cached
            if (!LocalizedStringCache.TryGetValue(localizedTooltip.Key, out var localizedText))
            {
                // If not cached, fetch and cache it
                localizedText =
                    LocalizationSettings.StringDatabase.GetLocalizedString("RandomWalkTooltips", localizedTooltip.Key);
                LocalizedStringCache[localizedTooltip.Key] = localizedText;

                Debug.Log($"Localized text for key '{localizedTooltip.Key}': {localizedText}");
            }

            // Assign the tooltip to the label
            var content = new GUIContent(label.text, localizedText);
            EditorGUI.PropertyField(position, property, content, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
#endif