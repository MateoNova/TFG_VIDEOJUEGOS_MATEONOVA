using System.Collections.Generic;
using UnityEngine;

namespace GraphBasedGenerator
{
    [OpenGraphEditor]
    public class GraphBasedGenerator : BaseGenerator
    {
        private GraphGeneratorView _graphView;
        
        /// <summary>
        /// Attribute to specify the scaling factor for the graph connections.
        /// </summary>
        private float _scalingFactor = 0.1f;

        public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap)
            {
                tilemapPainter.ResetAllTiles();
            }

            _graphView = GraphWindow.getGraphGeneratorView();

            if (_graphView == null)
            {
                Debug.LogError("GraphGeneratorView not found.");
                return;
            }

            // Guardamos las posiciones ajustadas para cada nodo.
            var adjustedPositions = new Dictionary<GraphNode, Vector2Int>();

            // Cargar habitaciones con posiciones ajustadas
            foreach (var node in _graphView.nodes)
            {
                if (node is not GraphNode graphNode) continue;
                
                var por = graphNode.GetPosition();
                var originalPos = new Vector2(por.x, -por.y);
                var adjustedPos = originalPos * _scalingFactor;
                var gridPos =
                    new Vector2Int(Mathf.RoundToInt(adjustedPos.x), Mathf.RoundToInt(adjustedPos.y));
                adjustedPositions[graphNode] = gridPos;

                Debug.Log("Loading tilemap from " + graphNode.JsonFilePath + " at adjusted position " + gridPos);
                tilemapPainter.LoadTilemap(graphNode.JsonFilePath,
                    offset: new Vector3Int(gridPos.x, gridPos.y, 0),
                    clearBeforeLoading: false);
            }

            // Generar corredores entre nodos usando las posiciones ajustadas
            foreach (var edge in _graphView.edges)
            {
                if (edge is null) continue;
                
                var outputPort = edge.output;
                var inputPort = edge.input;
                if (outputPort == null || inputPort == null) continue;

                if (outputPort.node is not GraphNode sourceNode || inputPort.node is not GraphNode targetNode) continue;
                var start = adjustedPositions[sourceNode];
                var end = adjustedPositions[targetNode];
                GenerateCorridor(start, end);
            }
        }

        /// <summary>
        /// Genera un corredor entre start y end asegurando que cada segmento esté entre 10 y 20 tiles.
        /// </summary>
        private void GenerateCorridor(Vector2Int start, Vector2Int end)
        {
            var distance = Vector2Int.Distance(start, end);
            var corridorPath = new List<Vector2Int>();

            switch (distance)
            {
                case > 20:
                {
                    // If the distance is greater than 20, we create segments of 20 tiles.
                    var current = start;
                    while (Vector2Int.Distance(current, end) > 20)
                    {
                        var direction = ((Vector2)(end - start)).normalized;
                        // Calcula el siguiente punto a 20 tiles de distancia
                        var next = current + Vector2Int.RoundToInt(direction * 20);
                        corridorPath.AddRange(Pathfinding.FindPath(current, next));
                        current = next;
                    }

                    // Add the last segment to reach the end
                    corridorPath.AddRange(Pathfinding.FindPath(current, end));
                    break;
                }
                case < 10:
                {
                    // If the distance is less than 10, we extend the end point to 10 tiles.
                    var direction = ((Vector2)(end - start)).normalized;
                    var extendedEnd = start + Vector2Int.RoundToInt(direction * 10);
                    corridorPath.AddRange(Pathfinding.FindPath(start, extendedEnd));
                    break;
                }
                default:
                    corridorPath = Pathfinding.FindPath(start, end);
                    break;
            }

            tilemapPainter.PaintWalkableTiles(corridorPath);
        }


        public override void OpenGraphWindow()
        {
            GraphWindow.ShowWindow();
        }
    }
}


namespace GraphBasedGenerator
{
    public static class Pathfinding
    {
        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            // Implement a simple pathfinding algorithm (e.g., A* or Dijkstra)
            // For simplicity, this example uses a straight line path
            var path = new List<Vector2Int>();
            var current = start;
            while (current != end)
            {
                if (current.x < end.x) current.x++;
                else if (current.x > end.x) current.x--;
                else if (current.y < end.y) current.y++;
                else if (current.y > end.y) current.y--;
                path.Add(current);
            }

            return path;
        }
    }
}