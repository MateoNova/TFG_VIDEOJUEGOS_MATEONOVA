using UnityEngine;
    
    public class InstantiateCharacter : MonoBehaviour
    {
        [SerializeField] private GameObject characterPrefab;
    
        void Start()
        {
            Instantiate(characterPrefab, transform.position, Quaternion.identity);
        }
    }