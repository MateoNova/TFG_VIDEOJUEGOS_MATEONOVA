using System.Collections;
using UnityEngine;

namespace Character
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothSpeed = 0.125f;
        public Vector3 offset;

        private void Start()
        {
            StartCoroutine(FindPlayer());
        }

        private IEnumerator FindPlayer()
        {
            while (!target)
            {
                var player = FindFirstObjectByType<SimpleMovement>();
                if (player)
                {
                    target = player.transform;
                }

                yield return null;
            }
        }

        private void LateUpdate()
        {
            if (!target) return;
            
            var desiredPosition = target.position + offset;
            var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Rotate the camera to face the direction of the target's movement
            transform.rotation = target.rotation;
        }
    }
}