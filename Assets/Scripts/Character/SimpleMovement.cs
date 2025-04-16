using UnityEngine;

namespace Character
{
    public class SimpleMovement : MonoBehaviour
    {
        public float moveSpeed = 5f;
        private Vector2 _movement;
    
        void Update()
        {
            // Get input from WASD keys
            _movement.x = Input.GetAxisRaw("Horizontal");
            _movement.y = Input.GetAxisRaw("Vertical");
        }
    
        void FixedUpdate()
        {
            // Move the character
            transform.Translate(_movement * (moveSpeed * Time.fixedDeltaTime));
        }
    }
}