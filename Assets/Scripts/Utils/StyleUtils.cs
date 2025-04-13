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

        public static Button ButtonInRowContainer(string text, Action action, bool first = false)
        {
            return new Button(action)
            {
                text = text,
                style =
                {
                    height = 30,
                    marginLeft = first ? 5 : 0,
                    flexGrow = 1
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
                    alignItems = Align.Center
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
    }
}