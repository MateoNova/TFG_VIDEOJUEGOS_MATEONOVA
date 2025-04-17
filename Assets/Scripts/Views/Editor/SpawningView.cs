using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Reflection;
using Character;
using Controllers.Editor;
using Utils;

namespace Views.Editor
{
    /// <summary>
    /// Provides a UI for managing character spawning in the scene.
    /// Includes options for selecting a character prefab and setting the spawn point position.
    /// </summary>
    public class SpawningView
    {
        /// <summary>
        /// The controller responsible for handling spawning logic.
        /// </summary>
        private VisualElement _root;
        
        /// <summary>
        /// The controller responsible for handling spawning logic.
        /// </summary>
        private InstantiateCharacter _spawnPointInstance;
        
        /// <summary>
        /// The field info for the character prefab.
        /// </summary>
        private FieldInfo _prefabField;

        /// <summary>
        /// Creates the main UI for character spawning.
        /// </summary>
        /// <returns>A VisualElement containing the UI.</returns>
        public VisualElement CreateUI()
        {
            _root = new VisualElement();

            var actionsFoldout = StyleUtils.ModernFoldout("");
            actionsFoldout.SetLocalizedText("SpawningOptions", "SpawningTable");
            _root.Add(actionsFoldout);

            actionsFoldout.Add(CreateCharacterSelector());

            var spawnButton = new Button(EnableSpawnPointSelection);
            spawnButton.SetLocalizedText("SetSpawnpointInScene", "SpawningTable");
            actionsFoldout.Add(spawnButton);

            InitializeSpawnPointReference();

            return _root;
        }

        /// <summary>
        /// Finds the "SpawnPoint" GameObject in the scene and retrieves its InstantiateCharacter component.
        /// Uses reflection to access the private "characterPrefab" field.
        /// </summary>
        private void InitializeSpawnPointReference()
        {
            if (_spawnPointInstance != null)
                return;

            var spawnGo = GameObject.Find("SpawnPoint");
            if (spawnGo != null)
            {
                _spawnPointInstance = spawnGo.GetComponent<InstantiateCharacter>();
                if (_spawnPointInstance != null)
                {
                    _prefabField = typeof(InstantiateCharacter).GetField("characterPrefab",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                }
                else
                {
                    Debug.LogError("InstantiateCharacter component not found on SpawnPoint.");
                }
            }
            else
            {
                Debug.LogError("SpawnPoint GameObject not found in the scene.");
            }
        }

        /// <summary>
        /// Creates a UI element for selecting a character prefab.
        /// Displays a preview of the current prefab and allows the user to change it using an object picker.
        /// The label is placed above the preview, and both are aligned to the left.
        /// </summary>
        /// <returns>A VisualElement containing the prefab selector.</returns>
        private VisualElement CreateCharacterSelector()
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.FlexStart
                }
            };

            var label = StyleUtils.LabelForTile("");
            label.SetLocalizedText("CharacterLabel", "SpawningTable");
            container.Add(label);

            var previewContainer = new IMGUIContainer(() =>
            {
                GameObject currentPrefab = null;
                if (_spawnPointInstance != null && _prefabField != null)
                {
                    currentPrefab = _prefabField.GetValue(_spawnPointInstance) as GameObject;
                }

                var previewTexture = currentPrefab != null
                    ? AssetPreview.GetAssetPreview(currentPrefab) ??
                      EditorGUIUtility.ObjectContent(currentPrefab, typeof(GameObject)).image
                    : EditorGUIUtility.IconContent("GameObject Icon").image;

                var previewSize = Utils.Utils.GetPreviewTileSize();

                if (GUILayout.Button(previewTexture, GUILayout.Width(previewSize), GUILayout.Height(previewSize)))
                {
                    EditorGUIUtility.ShowObjectPicker<GameObject>(currentPrefab, false, "t:GameObject", 12345);
                }

                if (Event.current.commandName != "ObjectSelectorUpdated") return;
                var picked = EditorGUIUtility.GetObjectPickerObject() as GameObject;

                if (picked == null || _spawnPointInstance == null || _prefabField == null) return;
                _prefabField.SetValue(_spawnPointInstance, picked);
                EditorUtility.SetDirty(_spawnPointInstance);
            });

            previewContainer.style.height = Utils.Utils.GetPreviewTileSize();
            container.Add(previewContainer);

            container.style.marginLeft = 5;
            container.style.marginBottom = 10;

            return container;
        }

        /// <summary>
        /// Enables spawn point selection mode, allowing the user to click in the scene to set the spawn point position.
        /// </summary>
        private void EnableSpawnPointSelection()
        {
            SpawningController.IsSettingSpawnPoint = true;
            EditorUtility.DisplayDialog(
                LocalizationUIHelper.SetLocalizedText("SpawnpointSelectionTitle", "SpawningTable"),
                LocalizationUIHelper.SetLocalizedText("SpawnpointSelectionMessage", "SpawningTable"),
                LocalizationUIHelper.SetLocalizedText("OkButton", "SpawningTable")
            );
        }
    }
}