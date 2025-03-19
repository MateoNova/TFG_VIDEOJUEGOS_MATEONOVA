using System.Collections.Generic;
using UnityEngine;


namespace GraphBasedGenerator
{
    [OpenGraphEditor]
    public class GraphBasedGenerator : BaseGenerator
    {
        private GraphGeneratorView _graphView;
        private float _scalingFactor = 0.1f;

        private readonly HashSet<Vector2Int> _occupiedDoorPositions = new();
        private readonly HashSet<Vector2Int> _allFloorPositions = new();


        public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            if (resetTilemap)
            {
                tilemapPainter.ResetAllTiles();
            }

            _occupiedDoorPositions.Clear();
            _allFloorPositions.Clear();

            _graphView = GraphWindow.getGraphGeneratorView();

            if (_graphView == null)
            {
                Debug.LogError("GraphGeneratorView not found.");
                return;
            }

            var roomDoors = PaintRooms();
            
            // 3. Generar corredores entre rooms
            foreach (var edge in _graphView.edges)
            {
                if (edge is null) continue;

                var outputPort = edge.output;
                var inputPort = edge.input;
                if (outputPort == null || inputPort == null) continue;

                if (outputPort.node is not GraphNode sourceNode || inputPort.node is not GraphNode targetNode)
                    continue;

                var room1Doors = roomDoors[sourceNode];
                var room2Doors = roomDoors[targetNode];
                GenerateCorridor(room1Doors, room2Doors);
            }
            
            WallGenerator.GenerateWalls(_allFloorPositions, tilemapPainter);
        }

        private Dictionary<GraphNode, List<Vector2Int>> PaintRooms()
        {
            var roomDoors = new Dictionary<GraphNode, List<Vector2Int>>();
            foreach (var node in _graphView.nodes)
            {
                if (node is not GraphNode graphNode) continue;

                var por = graphNode.GetPosition();
                var originalPos = new Vector2(por.x, -por.y);
                var adjustedPos = originalPos * _scalingFactor;
                var gridPos = new Vector2Int(Mathf.RoundToInt(adjustedPos.x), Mathf.RoundToInt(adjustedPos.y));

                // Pinta la habitación en el Tilemap
                tilemapPainter.LoadTilemap(
                    graphNode.JsonFilePath,
                    offset: new Vector3Int(gridPos.x, gridPos.y, 0),
                    clearBeforeLoading: false
                );

                // 2. Obtenemos posiciones de puertas y suelo
                var doors = GetDoorPositions(graphNode.JsonFilePath, gridPos);
                GetFloorPositions(graphNode.JsonFilePath, gridPos);
                roomDoors[graphNode] = doors;
            }

            return roomDoors;
        }

        private void GetFloorPositions(string jsonFilePath, Vector2Int offset)
        {
            var json = System.IO.File.ReadAllText(jsonFilePath);
            var tilemapData = JsonUtility.FromJson<TilemapData>(json);

            foreach (var tile in tilemapData.walkableTiles)
            {
                _allFloorPositions.Add(new Vector2Int(tile.position.x, tile.position.y) + offset);
            }
        }


        /// <summary>
        /// Obtiene las posiciones de las puertas de una habitación a partir del JSON.
        /// </summary>
        private List<Vector2Int> GetDoorPositions(string jsonFilePath, Vector2Int offset)
        {
            var doorPositions = new List<Vector2Int>();
            var json = System.IO.File.ReadAllText(jsonFilePath);
            var tilemapData = JsonUtility.FromJson<TilemapData>(json);

            foreach (var tile in tilemapData.doorTiles)
            {
                doorPositions.Add(new Vector2Int(tile.position.x, tile.position.y) + offset);
            }

            return doorPositions;
        }

        /// <summary>
        /// Genera un corredor entre las puertas de dos habitaciones.
        /// Se ordenan todas las parejas de puertas por distancia y se intenta generar el corredor
        /// con A* en cada pareja hasta encontrar una que funcione.
        /// </summary>
        private void GenerateCorridor(
            List<Vector2Int> room1Doors,
            List<Vector2Int> room2Doors
        )
        {
            // 1. Creamos la lista de parejas de puertas ordenadas por distancia
            var candidatePairs = new List<(Vector2Int door1, Vector2Int door2, float distance)>();
            foreach (var door1 in room1Doors)
            {
                foreach (var door2 in room2Doors)
                {
                    var distance = Vector2Int.Distance(door1, door2);
                    candidatePairs.Add((door1, door2, distance));
                }
            }

            candidatePairs.Sort((a, b) => a.distance.CompareTo(b.distance));

            // 2. Probamos cada pareja para ver si se puede trazar el corredor con A*
            foreach (var candidate in candidatePairs)
            {
                // Evitar usar puertas ya ocupadas
                if (_occupiedDoorPositions.Contains(candidate.door1) ||
                    _occupiedDoorPositions.Contains(candidate.door2))
                    continue;

                var corridorPath = Pathfinding.FindPath(candidate.door1, candidate.door2, tilemapPainter);
                if (corridorPath == null || corridorPath.Count == 0)
                    continue;

                // Marcamos estas puertas como usadas
                _occupiedDoorPositions.Add(candidate.door1);
                _occupiedDoorPositions.Add(candidate.door2);

                // Pintamos el corredor en el tilemap walkable
                tilemapPainter.PaintWalkableTiles(corridorPath);
                
                //add corridor to floor positions
                foreach (var pos in corridorPath)
                {
                    _allFloorPositions.Add(pos);
                }
                // --- Aquí viene la clave ---
                // Construimos el set de "suelo" para ESTE corredor
                var corridorFloorPositions = new HashSet<Vector2Int>(corridorPath);

                // Agregamos las puertas
                // corridorFloorPositions.Add(candidate.door1);
                // corridorFloorPositions.Add(candidate.door2);

                corridorPath.Remove(candidate.door1);
                corridorPath.Remove(candidate.door2);

                //remove the tile next to each door
                corridorPath.Remove(candidate.door1 + new Vector2Int(1, 0));
                corridorPath.Remove(candidate.door1 + new Vector2Int(-1, 0));

                

                // Pintamos las puertas en el tilemap
                tilemapPainter.PaintDoorTiles(new List<Vector2Int> { candidate.door1, candidate.door2 });

                // Termina aquí (ya construimos un corredor válido)
                return;
            }

            Debug.LogWarning("No se pudo generar un corredor entre las habitaciones con las puertas disponibles.");
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
        // Clase auxiliar para A*
        private class Node
        {
            public Vector2Int position;
            public float g; // costo desde el inicio
            public float f; // g + h
            public Node parent;
        }

        /// <summary>
        /// Implementa A* para encontrar un camino desde start hasta end, evitando celdas con tiles (paredes, puertas, etc.).
        /// Se permite que las celdas de inicio y fin tengan puertas.
        /// </summary>
        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, TilemapPainter painter)
        {
            var openList = new List<Node>();
            var closedSet = new HashSet<Vector2Int>();

            Node startNode = new Node { position = start, g = 0, f = Heuristic(start, end), parent = null };
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                // Selecciona el nodo con menor f
                openList.Sort((a, b) => a.f.CompareTo(b.f));
                Node current = openList[0];

                if (current.position == end)
                {
                    return ReconstructPath(current);
                }

                openList.Remove(current);
                closedSet.Add(current.position);

                // Vecinos en 4 direcciones
                foreach (var dir in new Vector2Int[]
                         {
                             new Vector2Int(0, 1),
                             new Vector2Int(0, -1),
                             new Vector2Int(1, 0),
                             new Vector2Int(-1, 0)
                         })
                {
                    Vector2Int neighborPos = current.position + dir;
                    if (closedSet.Contains(neighborPos))
                        continue;


                    if (!IsWalkable(neighborPos, painter, start, end))
                        continue;

                    float tentativeG = current.g + 1;
                    Node neighbor = openList.Find(n => n.position == neighborPos);
                    if (neighbor == null)
                    {
                        neighbor = new Node
                        {
                            position = neighborPos,
                            g = tentativeG,
                            f = tentativeG + Heuristic(neighborPos, end),
                            parent = current
                        };
                        openList.Add(neighbor);
                    }
                    else if (tentativeG < neighbor.g)
                    {
                        neighbor.g = tentativeG;
                        neighbor.f = tentativeG + Heuristic(neighborPos, end);
                        neighbor.parent = current;
                    }
                }
            }

            // Si no se encuentra camino, se devuelve una lista vacía.
            return new List<Vector2Int>();
        }

        // Heurística: distancia Manhattan
        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        // Reconstruye el camino desde el nodo final hasta el inicio.
        private static List<Vector2Int> ReconstructPath(Node endNode)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Node current = endNode;
            while (current != null)
            {
                path.Add(current.position);
                current = current.parent;
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Determina si la celda pos es transitable. Se consideran obstáculos las celdas que ya tengan tiles
        /// en walkableTilemap, wallTilemap o doorTilemap, salvo si es la celda de inicio o fin.
        /// </summary>
        private static bool IsWalkable(Vector2Int pos, TilemapPainter painter, Vector2Int start, Vector2Int end)
        {
            Vector3Int cellPos = new Vector3Int(pos.x, pos.y, 0);
            // Si la posición es el inicio o fin, se permite aunque tenga puerta
            bool isEndpoint = pos == start || pos == end;

            bool walkable = painter.walkableTilemap.GetTile(cellPos) == null &&
                            painter.wallTilemap.GetTile(cellPos) == null;
            if (!isEndpoint)
            {
                walkable = walkable && painter.doorTilemap.GetTile(cellPos) == null;
            }

            return walkable;
        }
    }
}