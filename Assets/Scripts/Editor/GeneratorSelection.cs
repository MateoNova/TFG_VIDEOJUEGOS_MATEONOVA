using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor
{
    public class GeneratorSelection
    {
        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";

        private int _selectedGeneratorIndex;
        private List<BaseGenerator> _generators = new();
        private static GameObject _cachedPrefab;
        private List<string> _cachedGeneratorNames = new();

        public static Action OnGeneratorChanged;

        private VisualElement _container;

        internal BaseGenerator CurrentGenerator { get; private set; }

        private GameObject CachedGenerationManager { get; set; }


        public VisualElement CreateUI()
        {
            _container = Utils.CreateContainer();

            var foldout = new Foldout { text = "Generator Selection", value = true };

            if (_cachedGeneratorNames == null || _cachedGeneratorNames.Count == 0)
            {
                var helpLabel =
                    Utils.CreateHelpLabel(
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

        public void SelectGenerator(int index)
        {
            if (index >= 0 && index < _generators.Count)
            {
                _selectedGeneratorIndex = index;
                CurrentGenerator = _generators[_selectedGeneratorIndex];
                OnGeneratorChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning("Índice de generador inválido.");
            }
        }

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

        private List<string> GetGeneratorNames()
        {
            List<string> names = new List<string>();
            foreach (var generator in _generators)
            {
                if (generator)
                {
                    names.Add(generator.name);
                }
                else
                {
                    Debug.LogWarning("El generador es null.");
                }
            }

            return names;
        }

        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }

        public void RetrieveOrInitializeCachedGenerationManager()
        {
            CachedGenerationManager = RetrieveCachedGenerationManager() ?? InstantiateGenerationManager();
        }

        private static GameObject RetrieveCachedGenerationManager()
        {
            var cachedId = EditorPrefs.GetInt(CachedGenerationManagerIdKey, -1);
            if (cachedId != -1)
            {
                return EditorUtility.InstanceIDToObject(cachedId) as GameObject;
            }

            return null;
        }

        private GameObject InstantiateGenerationManager()
        {
            if (_cachedPrefab == null)
            {
                _cachedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            if (_cachedPrefab == null)
            {
                Debug.LogError($"Prefab no encontrado en la ruta: {PrefabPath}");
                return null;
            }

            CachedGenerationManager = (GameObject)PrefabUtility.InstantiatePrefab(_cachedPrefab);
            EditorPrefs.SetInt(CachedGenerationManagerIdKey, CachedGenerationManager.GetInstanceID());
            return CachedGenerationManager;
        }

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
    }
}