using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace Utils
{
    public static class LocalizationUIHelper
    {
        /// <summary>
        /// Actualiza el texto de un Label en base a la clave y tabla proporcionadas, y se suscribe al cambio de idioma.
        /// </summary>
        public static void SetLocalizedText(this Label label, string key, string tableName = "DefaultTable")
        {
            void UpdateLabel()
            {
                label.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            // Asigna el texto inicial.
            UpdateLabel();

            // Nos suscribimos para que, al cambiar la configuración del idioma, se actualice el texto.
            LocalizationSettings.SelectedLocaleChanged += locale => UpdateLabel();
        }

        /// <summary>
        /// Actualiza el texto de un Button en base a la clave y tabla proporcionadas, y se suscribe al cambio de idioma.
        /// </summary>
        public static void SetLocalizedText(this Button button, string key, string tableName = "DefaultTable")
        {
            void UpdateButton()
            {
                button.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            // Asigna el texto inicial.
            UpdateButton();
            // Nos suscribimos para que, al cambiar el idioma, se actualice el texto.
            LocalizationSettings.SelectedLocaleChanged += locale => UpdateButton();
        }

        /// <summary>
        /// Actualiza el texto de un Foldout en base a la clave y tabla proporcionadas, y se suscribe al cambio de idioma.
        /// </summary>
        public static void SetLocalizedText(this Foldout foldout, string key, string tableName = "DefaultTable")
        {
            void UpdateFoldout()
            {
                foldout.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            UpdateFoldout();
            LocalizationSettings.SelectedLocaleChanged += locale => UpdateFoldout();
        }

        /// <summary>
        /// Actualiza el texto de un DropdownField en base a la clave y tabla proporcionadas.
        /// Se asume que el DropdownField mostrará un título o etiqueta.
        /// </summary>
        public static void SetLocalizedTitle(this DropdownField dropdown, string key, string tableName = "DefaultTable")
        {
            void UpdateDropdown()
            {
                // Se podría, por ejemplo, actualizar el placeholder o un label asociado
                dropdown.label = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            UpdateDropdown();
            LocalizationSettings.SelectedLocaleChanged += locale => UpdateDropdown();
        }

        /// <summary>
        /// Actualiza el texto de un Toggle en base a la clave y tabla proporcionadas, y se suscribe al cambio de idioma.
        /// </summary>
        public static void SetLocalizedText(this Toggle toggle, string key, string tableName = "DefaultTable")
        {
            void UpdateToggle()
            {
                toggle.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            // Asigna el texto inicial.
            UpdateToggle();

            // Nos suscribimos para que, al cambiar el idioma, se actualice el texto.
            LocalizationSettings.SelectedLocaleChanged += locale => UpdateToggle();
        }
        
        /// <summary>
        /// Updates the tooltip of a VisualElement based on the provided key and table, and subscribes to locale changes.
        /// </summary>
        public static void SetLocalizedTooltip(this VisualElement element, string key, string tableName = "DefaultTable")
        {
            void UpdateTooltip()
            {
                element.tooltip = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            }

            // Assign the initial tooltip.
            UpdateTooltip();

            // Subscribe to locale changes to update the tooltip dynamically.
            LocalizationSettings.SelectedLocaleChanged += locale => UpdateTooltip();
        }
        
        public static string SetLocalizedText(string key, string tableName = "DefaultTable")
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
        }
    }
}