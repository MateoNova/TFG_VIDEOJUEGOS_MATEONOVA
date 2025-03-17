using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor
{
    /// <summary>
    /// Manages the selection of generators within the editor.
    /// </summary>
    public class GeneratorSelection
    {
        #region Fields

        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";

        private int _selectedGeneratorIndex;
        private List<BaseGenerator> _generators = new();
        private static GameObject _cachedPrefab;
        private List<string> _cachedGeneratorNames = new();

        public static Action OnGeneratorChanged;
        public static Action<bool> ShowButtonOpenGraphWindow;

        private VisualElement _container;

        internal BaseGenerator CurrentGenerator { get; private set; }

        private GameObject CachedGenerationManager { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the UI for the generator selection.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the UI elements.</returns>
        public VisualElement CreateUI()
        {
            _container = StyleUtils.SimpleContainer();

            var foldout = new Foldout { text = "Generator Selection", value = true };

            if (_cachedGeneratorNames == null || _cachedGeneratorNames.Count == 0)
            {
                var helpLabel =
                    StyleUtils.HelpLabel(
                        "No generators found in the scene. Click the initialize button to search for them.");

                foldout.Add(helpLabel);
            }
            else
            {
                var dropdown = new DropdownField("Select Generator", _cachedGeneratorNames,
                    _cachedGeneratorNames[_selectedGeneratorIndex]);

                dropdown.RegisterValueChangedCallback(evt =>
                {
                    _selectedGeneratorIndex = _cachedGeneratorNames.IndexOf(evt.newValue);
                    SelectGenerator(_selectedGeneratorIndex);
                });

                foldout.Add(dropdown);
            }

            _container.Add(foldout);
            return _container;
        }

        /// <summary>
        /// Selects a generator by its index.
        /// </summary>
        /// <param name="index">The index of the generator to select.</param>
        public void SelectGenerator(int index)
        {
            if (index < 0 || index >= _generators.Count)
            {
                Debug.LogWarning("Index out of range.");
                return;
            }

            _selectedGeneratorIndex = index;
            CurrentGenerator = _generators[_selectedGeneratorIndex];

            // Check if the selected generator has the OpenGraphEditor attribute
            switch (Attribute.IsDefined(CurrentGenerator.GetType(), typeof(OpenGraphEditorAttribute)))
            {
                case true:
                    ShowButtonOpenGraphWindow?.Invoke(true);
                    break;
                case false when GeneratorSettings.GetShowOpenGraphButton():
                    ShowButtonOpenGraphWindow?.Invoke(false);
                    break;
            }

            OnGeneratorChanged?.Invoke();
        }

        /// <summary>
        /// Finds all generators in the scene.
        /// </summary>
        public void FindAllGenerators()
        {
            if (CachedGenerationManager != null)
            {
                _generators = new List<BaseGenerator>(CachedGenerationManager.GetComponentsInChildren<BaseGenerator>());
                _cachedGeneratorNames = GetGeneratorNames();
                if (_cachedGeneratorNames.Count > 0)
                {
                    EditorPrefs.SetString(CachedGeneratorNamesKey, string.Join(",", _cachedGeneratorNames));
                }
            }
            else
            {
                Debug.LogWarning("Generation Manager no encontrado en la escena.");
                ClearGeneratorLists();
            }
        }

        /// <summary>
        /// Gets the names of all generators.
        /// </summary>
        /// <returns>A list of generator names.</returns>
        private List<string> GetGeneratorNames()
        {
            var names = new List<string>();
            foreach (var generator in _generators)
            {
                if (generator)
                {
                    names.Add(generator.name);
                }
                else
                {
                    Debug.LogWarning("Generator not found.");
                }
            }

            return names;
        }

        /// <summary>
        /// Clears the lists of generators and cached generator names.
        /// </summary>
        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }

        /// <summary>
        /// Retrieves or initializes the cached generation manager.
        /// </summary>
        public void RetrieveOrInitializeCachedGenerationManager()
        {
            CachedGenerationManager = RetrieveCachedGenerationManager() ?? InstantiateGenerationManager();
        }

        /// <summary>
        /// Retrieves the cached generation manager.
        /// </summary>
        /// <returns>The cached generation manager, or null if not found.</returns>
        private static GameObject RetrieveCachedGenerationManager()
        {
            var cachedId = EditorPrefs.GetInt(CachedGenerationManagerIdKey, -1);
            if (cachedId != -1)
            {
                return EditorUtility.InstanceIDToObject(cachedId) as GameObject;
            }

            return null;
        }

        /// <summary>
        /// Instantiates the generation manager from the prefab.
        /// </summary>
        /// <returns>The instantiated generation manager.</returns>
        private GameObject InstantiateGenerationManager()
        {
            if (_cachedPrefab == null)
            {
                _cachedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            if (_cachedPrefab == null)
            {
                Debug.LogError($"Prefab wasn't found in route: {PrefabPath}");
                return null;
            }

            CachedGenerationManager = (GameObject)PrefabUtility.InstantiatePrefab(_cachedPrefab);
            EditorPrefs.SetInt(CachedGenerationManagerIdKey, CachedGenerationManager.GetInstanceID());
            return CachedGenerationManager;
        }

        /// <summary>
        /// Clears the cached data.
        /// </summary>
        public void ClearCacheData()
        {
            if (CachedGenerationManager)
            {
                Object.DestroyImmediate(CachedGenerationManager);
            }

            CachedGenerationManager = null;
            _cachedPrefab = null;
            CurrentGenerator = null;
            _cachedGeneratorNames.Clear();
            _generators.Clear();
            _selectedGeneratorIndex = 0;

            _container.MarkDirtyRepaint();
        }

        #endregion
    }
}