using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using Controllers.Editor;
using Utils;
using StyleUtils = Utils.StyleUtils;

#if UNITY_EDITOR

namespace Views.Editor
{
    /// <summary>
    /// Represents the Sprite Renamer View in the Unity Editor.
    /// This class provides a UI for selecting an image, renaming sprites, and creating presets.
    /// </summary>
    public class SpriteRenamerView
    {
        /// <summary>
        /// The controller responsible for handling sprite renaming logic.
        /// </summary>
        private readonly SpriteRenamerController _controller = new();

        /// <summary>
        /// The label for displaying the selected image path.
        /// </summary>
        private readonly LocalizedString _noImageString = new()
        {
            TableReference = "PresetsCreatorTable",
            TableEntryReference = "noImageSelected"
        };

        /// <summary>
        /// The label for displaying the selected image path.
        /// </summary>
        private readonly LocalizedString _imageSelectedString = new()
        {
            TableReference = "PresetsCreatorTable",
            TableEntryReference = "imageSelected"
        };

        /// <summary>
        /// The path to the selected image.
        /// </summary>
        private string _imagePath = "Assets/";

        /// <summary>
        /// The label for displaying the selected image path.
        /// </summary>
        private Label _selectedImageLabel;

        /// <summary>
        /// Creates the UI for the Sprite Renamer View.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements.</returns>
        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();

            var foldout = CreateFoldout();

            AddImageSelectionLabel(foldout);
            AddSelectImageButton(foldout);
            AddRenameButton(foldout);
            AddPresetButton(foldout);
            

            container.Add(foldout);
            return container;
        }

        /// <summary>
        /// Creates the main foldout element for grouping UI components.
        /// </summary>
        /// <returns>A <see cref="Foldout"/> element.</returns>
        private Foldout CreateFoldout()
        {
            var foldout = StyleUtils.ModernFoldout("");
            foldout.SetLocalizedText("presetsCreator", "PresetsCreatorTable");
            return foldout;
        }

        /// <summary>
        /// Adds the label for displaying the current image selection status to the foldout.
        /// </summary>
        /// <param name="foldout">The foldout to which the label will be added.</param>
        private void AddImageSelectionLabel(Foldout foldout)
        {
            _selectedImageLabel = StyleUtils.LabelForTile("");
            _selectedImageLabel.style.marginLeft = 2;
            foldout.Add(_selectedImageLabel);

            _noImageString.StringChanged += text =>
            {
                if (!string.IsNullOrEmpty(_imagePath) && _imagePath != "Assets/") return;

                _selectedImageLabel.text = text;
                _selectedImageLabel.MarkDirtyRepaint();
            };

            _imageSelectedString.StringChanged += text =>
            {
                if (string.IsNullOrEmpty(_imagePath) || _imagePath == "Assets/") return;

                _selectedImageLabel.text = $"{text} {_imagePath}";
                _selectedImageLabel.MarkDirtyRepaint();
            };

            _noImageString.RefreshString();
        }

        /// <summary>
        /// Adds the button for selecting an image to the foldout.
        /// </summary>
        /// <param name="foldout">The foldout to which the button will be added.</param>
        private void AddSelectImageButton(Foldout foldout)
        {
            var selectButton = new Button(() =>
            {
                var path = EditorUtility.OpenFilePanel("Select Image", "Assets", "png,jpg");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    _imagePath = "Assets" + path[Application.dataPath.Length..];
                    _imageSelectedString.RefreshString();
                }
                else
                {
                    _imagePath = "Assets/";
                    _noImageString.RefreshString();
                }
            });

            selectButton.SetLocalizedText("selectImage", "PresetsCreatorTable");
            foldout.Add(selectButton);
        }

        /// <summary>
        /// Adds the button for renaming sprites to the foldout.
        /// </summary>
        /// <param name="foldout">The foldout to which the button will be added.</param>
        private void AddRenameButton(Foldout foldout)
        {
            var renameButton = new Button(() =>
            {
                if (string.IsNullOrEmpty(_imagePath) || _imagePath == "Assets/")
                {
                    ShowErrorDialog("selectImageError");
                    return;
                }

                var ok = _controller.RenameSprites(_imagePath);
                ShowResultDialog(ok, "renameSuccess", "renameError");
            });

            renameButton.SetLocalizedText("renameSprites", "PresetsCreatorTable");
            foldout.Add(renameButton);
        }

        /// <summary>
        /// Adds the button for creating a preset to the foldout.
        /// </summary>
        /// <param name="foldout">The foldout to which the button will be added.</param>
        private void AddPresetButton(Foldout foldout)
        {
            var presetButton = new Button(() =>
            {
                if (string.IsNullOrEmpty(_imagePath) || _imagePath == "Assets/")
                {
                    ShowErrorDialog("selectImageError");
                    return;
                }

                var ok = _controller.CreatePreset(_imagePath);
                ShowResultDialog(ok, "presetSuccess", "presetError");
            });

            presetButton.SetLocalizedText("createPreset", "PresetsCreatorTable");
            foldout.Add(presetButton);
        }

        /// <summary>
        /// Displays an error dialog with a localized message.
        /// </summary>
        /// <param name="errorKey">The key for the localized error message.</param>
        private void ShowErrorDialog(string errorKey)
        {
            EditorUtility.DisplayDialog(
                LocalizationUIHelper.SetLocalizedText("errorTitle", "PresetsCreatorTable"),
                LocalizationUIHelper.SetLocalizedText(errorKey, "PresetsCreatorTable"),
                LocalizationUIHelper.SetLocalizedText("okButton", "PresetsCreatorTable")
            );
        }

        /// <summary>
        /// Displays a result dialog based on the success or failure of an operation.
        /// </summary>
        /// <param name="success">Indicates whether the operation was successful.</param>
        /// <param name="successKey">The key for the localized success message.</param>
        /// <param name="errorKey">The key for the localized error message.</param>
        private void ShowResultDialog(bool success, string successKey, string errorKey)
        {
            EditorUtility.DisplayDialog(
                LocalizationUIHelper.SetLocalizedText(success ? "successTitle" : "errorTitle", "PresetsCreatorTable"),
                LocalizationUIHelper.SetLocalizedText(success ? successKey : errorKey, "PresetsCreatorTable"),
                LocalizationUIHelper.SetLocalizedText("okButton", "PresetsCreatorTable")
            );
        }
    }
}

#endif