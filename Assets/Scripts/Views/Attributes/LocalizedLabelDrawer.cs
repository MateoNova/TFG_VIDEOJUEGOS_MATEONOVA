using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.Localization.Settings;
using UnityEditorInternal;
#endif

namespace Views.Attributes
{
    [CustomPropertyDrawer(typeof(LocalizedLabelAttribute))]
    public class LocalizedLabelDrawer : PropertyDrawer
    {
        private static float _sMaxLabelWidth;
        private const float Padding = 20f;
        private const float MinFieldWidth = 50f;

        static LocalizedLabelDrawer()
        {
#if UNITY_EDITOR
            LocalizationSettings.SelectedLocaleChanged += _ =>
            {
                _sMaxLabelWidth = 0f;
                InternalEditorUtility.RepaintAllViews();
            };
#endif
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not LocalizedLabelAttribute locAttr)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var text = Utils.LocalizationUIHelper.GetLocalizedString(locAttr.Key, locAttr.Table);
            var content = new GUIContent(text, label.tooltip);

            var labelTextWidth = GUI.skin.label.CalcSize(content).x + Padding;
            _sMaxLabelWidth = Mathf.Max(_sMaxLabelWidth, labelTextWidth);

            var maxAllowed = position.width - MinFieldWidth;
            var labelWidth = Mathf.Max(labelTextWidth, Mathf.Min(_sMaxLabelWidth, maxAllowed));

            EditorGUI.BeginProperty(position, content, property);

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth;

            EditorGUI.PropertyField(position, property, content, false);

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUI.EndProperty();
        }
    }
}