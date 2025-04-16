using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Controllers.Editor;
using Utils;
using StyleUtils = Utils.StyleUtils;

#if UNITY_EDITOR

namespace Views.Editor
{
    public class SpriteRenamerView
    {
        private readonly SpriteRenamerController _controller = new();
        private string _imagePath = "Assets/";

        public VisualElement CreateUI()
        {
            var container = StyleUtils.SimpleContainer();

            var foldout = StyleUtils.ModernFoldout("");
            foldout.SetLocalizedText("presetsCreator", "PresetsCreatorTable");

            var selectedImageLabel = StyleUtils.LabelForTile("No image selected");
            selectedImageLabel.style.marginLeft = 2;
            foldout.Add(selectedImageLabel);

            var selectButton = new Button(() =>
            {
                _imagePath = EditorUtility.OpenFilePanel("Select Image", "Assets", "png,jpg");
                if (!string.IsNullOrEmpty(_imagePath) && _imagePath.StartsWith(Application.dataPath))
                {
                    _imagePath = "Assets" + _imagePath[Application.dataPath.Length..];
                    selectedImageLabel.text = $"Selected: {_imagePath}";
                }
                else
                {
                    selectedImageLabel.text =
                        LocalizationUIHelper.SetLocalizedText("noImageSelected", "PresetsCreatorTable");
                }
            });
            selectButton.SetLocalizedText("selectImage", "PresetsCreatorTable");
            foldout.Add(selectButton);

            var renameButton = new Button(() =>
            {
                if (string.IsNullOrEmpty(_imagePath))
                {
                    EditorUtility.DisplayDialog(
                        LocalizationUIHelper.SetLocalizedText("errorTitle", "PresetsCreatorTable"),
                        LocalizationUIHelper.SetLocalizedText("selectImageError", "PresetsCreatorTable"),
                        LocalizationUIHelper.SetLocalizedText("okButton", "PresetsCreatorTable")
                    );
                    return;
                }

                if (_controller.RenameSprites(_imagePath))
                {
                    EditorUtility.DisplayDialog(
                        LocalizationUIHelper.SetLocalizedText("successTitle", "PresetsCreatorTable"),
                        LocalizationUIHelper.SetLocalizedText("renameSuccess", "PresetsCreatorTable"),
                        LocalizationUIHelper.SetLocalizedText("okButton", "PresetsCreatorTable")
                    );
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        LocalizationUIHelper.SetLocalizedText("errorTitle", "PresetsCreatorTable"),
                        LocalizationUIHelper.SetLocalizedText("renameError", "PresetsCreatorTable"),
                        LocalizationUIHelper.SetLocalizedText("okButton", "PresetsCreatorTable")
                    );
                }
            });
            
            renameButton.SetLocalizedText("renameSprites", "PresetsCreatorTable");
            foldout.Add(renameButton);
            
            var presetButton = new Button(() =>
            {
                if (string.IsNullOrEmpty(_imagePath))
                {
                    EditorUtility.DisplayDialog("Error", "Please select an image first.", "OK");
                    return;
                }
                if (_controller.CreatePreset(_imagePath))
                {
                    EditorUtility.DisplayDialog("Success", "Preset created successfully.", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to create preset.", "OK");
                }
            });
            presetButton.SetLocalizedText("createPreset", "PresetsCreatorTable");
            foldout.Add(presetButton);

            container.Add(foldout);

            return container;
        }
    }
}

#endif