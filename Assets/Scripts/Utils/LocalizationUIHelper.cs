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
            label.schedule.Execute(() => UpdateLabel());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                label.schedule.Execute(() => UpdateLabel());
            return;

            void UpdateLabel()
            {
                label.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }
        }

        /// <summary>
        /// Updates the text of a Button based on the provided key and table, and subscribes to language changes.
        /// </summary>
        /// <param name="button">The Button to update.</param>
        /// <param name="key">The localization key to retrieve the text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        public static void SetLocalizedText(this Button button, string key, string tableName = "DefaultTable")
        {
            button.schedule.Execute(() => UpdateButton());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                button.schedule.Execute(() => UpdateButton());
            return;

            void UpdateButton()
            {
                button.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }
        }

        /// <summary>
        /// Updates the text of a Foldout based on the provided key and table, and subscribes to language changes.
        /// </summary>
        /// <param name="foldout">The Foldout to update.</param>
        /// <param name="key">The localization key to retrieve the text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        public static void SetLocalizedText(this VisualElement expander, string key, string tableName = "DefaultTable")
        {
            // Find the title label (assuming it's always the second child of the header button)
            var header = expander.Q<Button>();
            var titleLabel = header?.Q<Label>(null, (string[])null); // This gets the first Label (the arrow), so get the second:
            if (header != null && header.childCount > 1)
            {
                titleLabel = header[1] as Label;
            }
        
            void UpdateTitle()
            {
                if (titleLabel != null)
                    titleLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }
        
            expander.schedule.Execute(UpdateTitle);
            LocalizationSettings.SelectedLocaleChanged += _ => expander.schedule.Execute(UpdateTitle);
        }

        /// <summary>
        /// Updates the label (title) of a DropdownField based on the provided key and table.
        /// </summary>
        /// <param name="dropdown">The DropdownField to update.</param>
        /// <param name="key">The localization key to retrieve the label text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        public static void SetLocalizedTitle(this DropdownField dropdown, string key, string tableName = "DefaultTable")
        {
            dropdown.schedule.Execute(() => UpdateDropdown());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                dropdown.schedule.Execute(() => UpdateDropdown());
            return;

            void UpdateDropdown()
            {
                dropdown.label = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }
        }

        /// <summary>
        /// Updates the text of a Toggle based on the provided key and table, and subscribes to language changes.
        /// </summary>
        /// <param name="toggle">The Toggle to update.</param>
        /// <param name="key">The localization key to retrieve the text.</param>
        /// <param name="tableName">The name of the localization table (default is "DefaultTable").</param>
        public static void SetLocalizedText(this Toggle toggle, string key, string tableName = "DefaultTable")
        {
            toggle.schedule.Execute(() => UpdateToggle());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                toggle.schedule.Execute(() => UpdateToggle());
            return;

            void UpdateToggle()
            {
                toggle.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }
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
            element.schedule.Execute(() => UpdateTooltip());
            LocalizationSettings.SelectedLocaleChanged += locale =>
                element.schedule.Execute(() => UpdateTooltip());
            return;

            void UpdateTooltip()
            {
                element.tooltip = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }
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

        public static string GetGeneratorsDisplayName(string tableName)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                LocalizationSettings.StringDatabase.GetTable(tableName) != null ? tableName : "DefaultTable",
                "DisplayName");
        }

        public static string GetLocalizedString(string localizedAttrKey, string localizedAttrTable)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                LocalizationSettings.StringDatabase.GetTable(localizedAttrTable)
                    ? localizedAttrTable
                    : "DefaultTable", localizedAttrKey);
        }
    }
}