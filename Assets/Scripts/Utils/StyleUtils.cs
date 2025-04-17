using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils
{
    public static class StyleUtils
    {
        public static ScrollView SimpleScrollView()
        {
            return new ScrollView
            {
                style =
                {
                    flexGrow = 1
                }
            };
        }

        public static VisualElement RowButtonContainer()
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginTop = 5,
                    marginBottom = 5
                }
            };
        }
        
        public static VisualElement ColumnButtonContainer()
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    marginTop = 5,
                    marginBottom = 5
                }
            };
        }

        public static Button ButtonInRowContainer(string text, Action action, bool first = false)
        {
            return new Button(action)
            {
                text = text,
                style =
                {
                    height = 30,
                    marginLeft = first ? 5 : 0,
                    flexGrow = 1,
                }
            };
        }

        public static VisualElement TileContainer()
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Center,
                    marginBottom = 10
                }
            };
        }

        public static VisualElement HorizontalContainerWrapped()
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap
                }
            };
        }

        public static Label LabelForTile(string labelText)
        {
            return new Label(labelText)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 10,
                    marginBottom = 5
                }
            };
        }

        public static VisualElement SimpleContainer()
        {
            return new VisualElement
            {
                style =
                {
                    marginBottom = 10
                }
            };
        }

        public static Label HelpLabel(string text)
        {
            return new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Italic
                }
            };
        }

        public static VisualElement HorizontalContainerCentered()
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginTop = 5
                }
            };
        }

        public static Label LabelForToggle(string text)
        {
            return new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginRight = 10
                }
            };
        }

        public static Label LabelForIntField(string text)
        {
            return new Label(text)
            {
                style =
                {
                    width = 50,
                    marginRight = 5
                }
            };
        }

        public static IntegerField SimpleIntField(int value)
        {
            return new IntegerField
            {
                value = value,
                style =
                {
                    width = 30
                }
            };
        }

        public static Toggle SimpleToggle(string text, bool value, string tooltip)
        {
            return new Toggle(text)
            {
                tooltip = tooltip,
                value = value
            };
        }

        public static Foldout ModernFoldout(string text, bool expanded = true)
        {
            var foldout = new Foldout
            {
                value = expanded,
                text = text,
                style =
                {
                    marginTop = 5,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1
                }
            };

            // Apply the font size and style only after the Foldout is fully initialized
            foldout.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var label = foldout.Q<Label>();

                if (label == null) return;

                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                label.style.fontSize = 16;
            });

            return foldout;
        }

        public static Foldout ModernSubFoldout(string text, bool expanded = true)
        {
            var foldout = new Foldout
            {
                value = expanded,
                text = text,
                style =
                {
                    marginTop = 3,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                }
            };

            // Apply the font size and style only after the Foldout is fully initialized
            foldout.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var label = foldout.Q<Label>();

                if (label == null) return;

                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                label.style.fontSize = 14;
            });

            return foldout;
        }

        public static DropdownField SimpleDropdown()
        {
            var dropdown = new DropdownField
            {
                style =
                {
                    marginTop = 5,
                    marginBottom = 5
                }
            };

            dropdown.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var label = dropdown.Q<Label>();
                if (label != null)
                {
                    label.style.fontSize = 12;
                }
            });

            return dropdown;
        }
    }
}