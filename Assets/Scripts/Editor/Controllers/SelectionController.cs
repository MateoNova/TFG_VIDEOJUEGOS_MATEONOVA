using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Models;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Controllers
{
    public class SelectionController
    {
        private const string CachedGeneratorNamesKey = "CachedGeneratorNames";
        private const string CachedGenerationManagerIdKey = "CachedGenerationManagerId";
        private const string PrefabPath = "Assets/Prefabs/GenerationManager.prefab";

        private int _selectedGeneratorIndex;
        private List<BaseGenerator> _generators = new();
        private static GameObject _cachedPrefab;
        private List<string> _cachedGeneratorNames = new();

        // Propiedades públicas para la vista
        public List<string> CachedGeneratorNames() => _cachedGeneratorNames;
        public int SelectedGeneratorIndex() => _selectedGeneratorIndex;

        // Se subscriben al evento de inicialización mediante el EventBus (por ejemplo, en el constructor o inicialización)
        public SelectionController()
        {
            EventBus.InitScene += InitScene;
            EventBus.ClearCachedData += ClearCacheData;
        }

        // Método invocado al cambiar el valor del dropdown
        public void ChangeGenerator(string newGeneratorName)
        {
            _selectedGeneratorIndex = _cachedGeneratorNames.IndexOf(newGeneratorName);
            SelectGenerator(_selectedGeneratorIndex);
        }

        private void InitScene()
        {
            RetrieveOrInitializeCachedGenerationManager();
            FindAllGenerators();
            SelectGenerator(0);
        }

        private void SelectGenerator(int index)
        {
            if (index < 0 || index >= _generators.Count)
            {
                Debug.LogWarning("Index out of range.");
                return;
            }

            _selectedGeneratorIndex = index;
            BaseGenerator selectedGenerator = _generators[_selectedGeneratorIndex];
            GeneratorService.Instance.SetCurrentGenerator(selectedGenerator);

            // Se detecta si el generador tiene el atributo OpenGraphEditor y se notifica para mostrar u ocultar el botón
            if (Attribute.IsDefined(selectedGenerator.GetType(), typeof(OpenGraphEditorAttribute)))
            {
                EventBus.OnToggleOpenGraphButton(true);
            }
            else
            {
                // Se puede tener una configuración predeterminada en GeneratorSettings, en este ejemplo se envía false.
                EventBus.OnToggleOpenGraphButton(false);
            }
        }

        private void FindAllGenerators()
        {
            GameObject generationManager = RetrieveCachedGenerationManager();
            if (generationManager != null)
            {
                _generators = new List<BaseGenerator>(generationManager.GetComponentsInChildren<BaseGenerator>());
                _cachedGeneratorNames = _generators.Where(g => g != null).Select(g => g.name).ToList();

                if (_cachedGeneratorNames.Count > 0)
                {
                    EditorPrefs.SetString(CachedGeneratorNamesKey, string.Join(",", _cachedGeneratorNames));
                }
            }
            else
            {
                Debug.LogWarning("Generation Manager not found in the scene.");
                ClearGeneratorLists();
            }
        }

        private void ClearGeneratorLists()
        {
            _generators.Clear();
            _cachedGeneratorNames.Clear();
            EditorPrefs.DeleteKey(CachedGeneratorNamesKey);
        }

        private void RetrieveOrInitializeCachedGenerationManager()
        {
            GameObject generationManager = RetrieveCachedGenerationManager() ?? InstantiateGenerationManager();
        }

        private static GameObject RetrieveCachedGenerationManager()
        {
            int cachedId = EditorPrefs.GetInt(CachedGenerationManagerIdKey, -1);
            return cachedId != -1 ? EditorUtility.InstanceIDToObject(cachedId) as GameObject : null;
        }

        private GameObject InstantiateGenerationManager()
        {
            if (_cachedPrefab == null)
            {
                _cachedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            if (_cachedPrefab == null)
            {
                Debug.LogError($"Prefab wasn't found at path: {PrefabPath}");
                return null;
            }

            GameObject generationManager = (GameObject)PrefabUtility.InstantiatePrefab(_cachedPrefab);
            EditorPrefs.SetInt(CachedGenerationManagerIdKey, generationManager.GetInstanceID());
            return generationManager;
        }

        private void ClearCacheData()
        {
            GameObject cachedGenerationManager = RetrieveCachedGenerationManager();
            if (cachedGenerationManager)
            {
                Object.DestroyImmediate(cachedGenerationManager);
            }

            EditorPrefs.DeleteKey(CachedGenerationManagerIdKey);
            GeneratorService.Instance.SetCurrentGenerator(null);
            _cachedGeneratorNames.Clear();
            _generators.Clear();
            _selectedGeneratorIndex = 0;
        }
    }
}