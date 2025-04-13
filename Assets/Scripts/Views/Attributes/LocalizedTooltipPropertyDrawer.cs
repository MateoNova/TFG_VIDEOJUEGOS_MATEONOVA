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

        static LocalizedTooltipPropertyDrawer()
        {
            // Subscribe to locale change event
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        private static void OnLocaleChanged(UnityEngine.Localization.Locale locale)
        {
            // Clear the cache when the locale changes
            LocalizedStringCache.Clear();
        }

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

            // Construct a unique cache key using the table name and key
            var cacheKey = $"{localizedTooltip.TableName}:{localizedTooltip.Key}";

            // Check if the localized string is already cached
            if (!LocalizedStringCache.TryGetValue(cacheKey, out var localizedText))
            {
                // If not cached, fetch and cache it
                localizedText =
                    LocalizationSettings.StringDatabase.GetLocalizedString(localizedTooltip.TableName,
                        localizedTooltip.Key);
                LocalizedStringCache[cacheKey] = localizedText;
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