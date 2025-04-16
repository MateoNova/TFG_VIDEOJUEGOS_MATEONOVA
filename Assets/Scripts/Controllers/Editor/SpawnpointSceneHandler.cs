using UnityEditor;
using UnityEngine;

namespace Controllers.Editor
{
    /// <summary>
    /// Handles the visualization and interaction with the SpawnPoint in the Scene view.
    /// This static class listens to Scene GUI events and updates the SpawnPoint's position
    /// when the user interacts with the scene.
    /// </summary>
    [InitializeOnLoad]
    public static class SpawnpointSceneHandler
    {
        private static GameObject _cachedSpawnPoint;

        /// <summary>
        /// Static constructor to initialize event subscriptions.
        /// </summary>
        static SpawnpointSceneHandler()
        {
            // Subscribe to the SceneView GUI event to handle custom drawing and interactions.
            SceneView.duringSceneGui += OnSceneGUI;
            // Subscribe to hierarchy changes to clear the cached SpawnPoint reference.
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        /// <summary>
        /// Clears the cached SpawnPoint reference when the hierarchy changes.
        /// </summary>
        private static void OnHierarchyChanged()
        {
            _cachedSpawnPoint = null;
        }

        /// <summary>
        /// Retrieves the SpawnPoint GameObject, using a cached reference if available.
        /// </summary>
        /// <returns>The SpawnPoint GameObject, or null if not found.</returns>
        private static GameObject GetCachedSpawnPoint()
        {
            if (_cachedSpawnPoint == null)
            {
                _cachedSpawnPoint = GameObject.Find("SpawnPoint");
            }

            return _cachedSpawnPoint;
        }

        /// <summary>
        /// Handles the SceneView GUI event to draw and interact with the SpawnPoint.
        /// </summary>
        /// <param name="sceneView">The current SceneView instance.</param>
        private static void OnSceneGUI(SceneView sceneView)
        {
            var spawnPoint = GetCachedSpawnPoint();
            if (spawnPoint != null)
            {
                // Draw a persistent yellow indicator for the SpawnPoint.
                Handles.color = Color.yellow;
                Handles.DrawSolidDisc(spawnPoint.transform.position, Vector3.forward, 0.5f);
                Handles.Label(spawnPoint.transform.position + Vector3.up, "SpawnPoint");
            }

            if (!SpawningController.IsSettingSpawnPoint)
                return;

            // Handle mouse interaction for setting the SpawnPoint position.
            var e = Event.current;

            // Convert mouse position to world coordinates.
            var mousePos = e.mousePosition;
            mousePos.y = sceneView.camera.pixelHeight - mousePos.y; // Invert Y-axis for 2D.
            var worldPos = sceneView.camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
            worldPos.z = 0f;

            // Draw a cyan indicator for the candidate SpawnPoint position.
            Handles.color = Color.cyan;
            Handles.DrawSolidDisc(worldPos, Vector3.forward, 0.5f);
            Handles.Label(worldPos + Vector3.up, "SpawnPoint Candidate");

            // Update the SpawnPoint position on left mouse click.
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (spawnPoint != null)
                {
                    spawnPoint.transform.position = worldPos;
                }
                else
                {
                    Debug.LogError("SpawnPoint not found in the scene.");
                }

                // Exit spawn point selection mode after the click.
                SpawningController.IsSettingSpawnPoint = false;
                e.Use();
                SceneView.RepaintAll();
            }
            
            sceneView.Repaint();
        }
    }
}