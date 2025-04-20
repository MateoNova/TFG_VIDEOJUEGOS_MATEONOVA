using UnityEngine;

namespace Character
{
    public class InstantiateCharacter : MonoBehaviour
    {
        [SerializeField] private GameObject characterPrefab;

        private void Start()
        {
            Instantiate(characterPrefab, transform.position, Quaternion.identity);
        }
    }
}