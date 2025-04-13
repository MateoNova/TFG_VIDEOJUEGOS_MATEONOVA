/*using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Views.Tooltips
{
    public class TooltipLocalizer
    {
        private const string StringTableName = "RandomWalkTooltips"; // Nombre de la String Table
        private LocalizedStringTable localizedStringTable;

        public TooltipLocalizer()
        {
            // Carga la String Table
            localizedStringTable = LocalizationSettings.StringDatabase.GetTableAsync(StringTableName).Result;
        }

        /// <summary>
        /// Obtiene el texto localizado para una clave específica.
        /// </summary>
        /// <param name="key">La clave de la traducción.</param>
        /// <returns>El texto localizado.</returns>
        public string GetTooltip(string key)
        {
            if (localizedStringTable == null)
            {
                Debug.LogError($"String Table '{StringTableName}' not found.");
                return string.Empty;
            }

            var entry = localizedStringTable.GetEntry(key);
            if (entry == null)
            {
                Debug.LogError($"Key '{key}' not found in String Table '{StringTableName}'.");
                return string.Empty;
            }

            return entry.GetLocalizedString();
        }
    }
}*/