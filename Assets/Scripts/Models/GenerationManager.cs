using UnityEngine;

namespace Models
{
    /// <summary>
    /// Singleton manager for generation-related operations.
    /// </summary>
    public class GenerationManager : MonoBehaviour
    {
        public static GenerationManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}