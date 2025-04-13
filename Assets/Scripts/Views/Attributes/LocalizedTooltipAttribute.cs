namespace Views.Attributes
{
    using UnityEngine;

    public class LocalizedTooltipAttribute : PropertyAttribute
    {
        /// <summary>
        /// The key used to fetch the localized string.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The name of the localization table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Creates a new localized tooltip with the specified key and table name.
        /// </summary>
        /// <param name="key">The key within the localization table.</param>
        /// <param name="tableName">The name of the localization table.</param>
        public LocalizedTooltipAttribute(string key, string tableName = "DefaultTable")
        {
            Key = key;
            TableName = tableName;
        }
    }
}