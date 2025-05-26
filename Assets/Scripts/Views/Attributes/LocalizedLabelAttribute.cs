using UnityEngine;

namespace Views.Attributes
{
    public class LocalizedLabelAttribute : PropertyAttribute
    {
        public string Key { get; }
        public string Table { get; }

        public LocalizedLabelAttribute(string key, string table)
        {
            Key = key;
            Table = table;
        }
    }
}