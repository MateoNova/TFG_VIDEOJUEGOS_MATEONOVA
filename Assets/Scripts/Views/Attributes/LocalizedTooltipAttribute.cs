namespace Views.Attributes
{
    using UnityEngine;

    public class LocalizedTooltipAttribute : PropertyAttribute
    {
        /// <summary>
        /// La llave que se usará para obtener el string localizado.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Crea un nuevo tooltip localizado con la llave indicada.
        /// </summary>
        /// <param name="key">La llave dentro de la tabla de localización.</param>
        public LocalizedTooltipAttribute(string key)
        {
            Key = key;
        }
    }
}