using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

namespace Utils
{
    /// <summary>
    /// Utility class providing helper methods for localizing UI elements in Unity.
    /// </summary>
    public static class LocalizationUIHelper
    {
        /// <summary>
        /// Updates the text of a Label based on the provided key and table, and subscribes to language changes.
        /// </summary>
        /// <param name="label">The Label to update.</param>
        /// <param name="key">The localization key to retrieve the text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        public static void SetLocalizedText(this Label label, string key, string tableName = "DefaultTable")
        {
            void UpdateLabel()
            {
                label.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            label.schedule.Execute(() => UpdateLabel());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                label.schedule.Execute(() => UpdateLabel());
        }

        /// <summary>
        /// Updates the text of a Button based on the provided key and table, and subscribes to language changes.
        /// </summary>
        /// <param name="button">The Button to update.</param>
        /// <param name="key">The localization key to retrieve the text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        public static void SetLocalizedText(this Button button, string key, string tableName = "DefaultTable")
        {
            void UpdateButton()
            {
                button.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            button.schedule.Execute(() => UpdateButton());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                button.schedule.Execute(() => UpdateButton());
        }

        /// <summary>
        /// Updates the text of a Foldout based on the provided key and table, and subscribes to language changes.
        /// </summary>
        /// <param name="foldout">The Foldout to update.</param>
        /// <param name="key">The localization key to retrieve the text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        public static void SetLocalizedText(this Foldout foldout, string key, string tableName = "DefaultTable")
        {
            void UpdateFoldout()
            {
                foldout.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            foldout.schedule.Execute(() => UpdateFoldout());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                foldout.schedule.Execute(() => UpdateFoldout());
        }

        /// <summary>
        /// Updates the label (title) of a DropdownField based on the provided key and table.
        /// </summary>
        /// <param name="dropdown">The DropdownField to update.</param>
        /// <param name="key">The localization key to retrieve the label text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        public static void SetLocalizedTitle(this DropdownField dropdown, string key, string tableName = "DefaultTable")
        {
            void UpdateDropdown()
            {
                dropdown.label = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            dropdown.schedule.Execute(() => UpdateDropdown());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                dropdown.schedule.Execute(() => UpdateDropdown());
        }

        /// <summary>
        /// Updates the text of a Toggle based on the provided key and table, and subscribes to language changes.
        /// </summary>
        /// <param name="toggle">The Toggle to update.</param>
        /// <param name="key">The localization key to retrieve the text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        public static void SetLocalizedText(this Toggle toggle, string key, string tableName = "DefaultTable")
        {
            void UpdateToggle()
            {
                toggle.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            toggle.schedule.Execute(() => UpdateToggle());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                toggle.schedule.Execute(() => UpdateToggle());
        }

        /// <summary>
        /// Updates the tooltip of a VisualElement based on the provided key and table, and subscribes to language changes.
        /// </summary>
        /// <param name="element">The VisualElement to update.</param>
        /// <param name="key">The localization key to retrieve the tooltip text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        public static void SetLocalizedTooltip(this VisualElement element, string key,
            string tableName = "DefaultTable")
        {
            void UpdateTooltip()
            {
                element.tooltip = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            element.schedule.Execute(() => UpdateTooltip());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                element.schedule.Execute(() => UpdateTooltip());
        }

        /// <summary>
        /// Retrieves the localized text based on the provided key and table.
        /// </summary>
        /// <param name="key">The localization key to retrieve the text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        /// <returns>The localized string.</returns>
        public static string SetLocalizedText(string key, string tableName = "DefaultTable")
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
        }
    }
}