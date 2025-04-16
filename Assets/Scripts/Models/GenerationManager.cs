using UnityEngine;

namespace Models
{
    public class GenerationManager : MonoBehaviour
    {
        public static GenerationManager Instance { get; private set; }

        private void Awake()
        {
            // Si ya existe una instancia y no es esta, se destruye la nueva
            if (Instance != null && Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            // Si quieres que persista al cambiar de escenas:
            DontDestroyOnLoad(gameObject);
        }
    }
}