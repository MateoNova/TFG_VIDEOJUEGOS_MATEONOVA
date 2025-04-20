using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SimpleMovement : MonoBehaviour
    {
        public float moveSpeed = 5f;
        private Vector2 _movement;
        private Rigidbody2D _rigidbody2D;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Capture input in Update
            _movement.x = Input.GetAxisRaw("Horizontal");
            _movement.y = Input.GetAxisRaw("Vertical");
        }

        private void FixedUpdate()
        {
            _rigidbody2D.linearVelocity = _movement.normalized * moveSpeed;
        }
    }
}