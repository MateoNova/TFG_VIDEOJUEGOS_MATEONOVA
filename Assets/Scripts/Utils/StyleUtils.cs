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
                    unityFontStyleAndWeight = FontStyle.Normal,
                    marginTop = 10,
                    marginBottom = 5,
                    fontSize = 13
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
                    marginTop = 5,
                    flexGrow = 1
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
                    marginRight = 5,
                    flexGrow = 0,
                    flexShrink = 0
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

        public static VisualElement ModernFoldout(string text, bool expanded = true)
        {
            // Contenedor raíz
            var expander = new VisualElement
            {
                style =
                {
                    marginTop = 5,
                    marginBottom = 5,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    paddingLeft = 4,
                    paddingRight = 4,
                    paddingTop = 2,
                    paddingBottom = 2
                }
            };

            // 1) Cabecera: un Toggle que actúa de “foldout header”
            var header = new Toggle(text)
            {
                value = expanded,
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 16,
                    marginBottom = 4
                }
            };
            expander.Add(header);

            // 2) Contenedor de contenido
            var content = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    display = expanded ? DisplayStyle.Flex : DisplayStyle.None
                }
            };
            expander.Add(content);

            // 3) Al cambiar el toggle mostramos/ocultamos el contenido
            header.RegisterValueChangedCallback(evt =>
            {
                content.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return content;
        }

        /// <summary>
        /// Crea un expander (similar a Foldout) sin propagar estilos a los hijos.
        /// </summary>
        /// <param name="title">Texto de la cabecera.</param>
        /// <param name="content">VisualElement donde añadirás tus controles.</param>
        /// <param name="expanded">Si empieza desplegado o no.</param>
        /// <returns>El VisualElement completo (cabecera + contenido).</returns>
        public static VisualElement SimpleExpander(string title, out VisualElement content, bool expanded = true)
        {
            var expander = new VisualElement
            {
                style =
                {
                    marginTop = 5,
                    marginBottom = 5,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    paddingLeft = 4,
                    paddingRight = 4,
                    paddingTop = 2,
                    paddingBottom = 2
                }
            };
        
            // Header with arrow and title
            var header = new Button
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 10,
                    marginBottom = 4,
                    backgroundColor = Color.clear,
                    borderBottomWidth = 0,
                    borderTopWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    paddingLeft = 2 // Add a bit of left padding
                }
            };
        
            var arrow = new Label(expanded ? "▼" : "▶")
            {
                style =
                {
                    width = 14, // Slightly wider for better alignment
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginRight = 4
                }
            };
            var titleLabel = new Label(title)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14
                }
            };
        
            header.Add(arrow);
            header.Add(titleLabel);
            expander.Add(header);
        
            content = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    display = expanded ? DisplayStyle.Flex : DisplayStyle.None,
                    marginLeft = 18 // width of arrow (14) + marginRight (4)
                }
            };
            expander.Add(content);
        
            var element = content;
            header.clicked += () =>
            {
                var isExpanded = element.style.display == DisplayStyle.Flex;
                element.style.display = isExpanded ? DisplayStyle.None : DisplayStyle.Flex;
                arrow.text = isExpanded ? "▶" : "▼";
            };
        
            return expander;
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

        public static Button DisplayChangeButton(bool condition, Action action)
        {
            return new Button(action)
            {
                style = { display = condition ? DisplayStyle.Flex : DisplayStyle.None }
            };
        }
    }
}