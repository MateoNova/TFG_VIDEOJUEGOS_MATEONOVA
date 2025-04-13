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
    }
}