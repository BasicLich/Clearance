using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Based on: https://github.com/AaronEdge/TilePathfinding/blob/master/NodeGrid.cs (I cleaned it up)

namespace Pathfinding {
    public class NodeGrid : MonoBehaviour {
        public Grid grid;
        public string solidTag = "solid";
        public Node[,] nodes;
        
        public Vector2Int worldSize;
        public Vector2Int offset;

        public static NodeGrid Instance;

        private void Awake() {
            Instance = this;
        }

        // Calculate nodes on game start.
        private void Start() {
            // Fetch tilemaps
            var tilemaps = grid.GetComponentsInChildren<Tilemap>();

            // Find the world size.
            foreach (var map in tilemaps) {
                map.CompressBounds();
                BoundsInt bounds = map.cellBounds;
                if (bounds.size.x > worldSize.x)
                    worldSize.x = bounds.size.x;
                if (bounds.size.y > worldSize.y)
                    worldSize.y = bounds.size.y;
                if (bounds.xMin < offset.x)
                    offset.x = bounds.xMin;
                if (bounds.yMin < offset.y)
                    offset.y = bounds.yMin;
            }

            // Invert
            offset *= -1;
            
            // Create nodes array
            nodes = new Node[worldSize.x, worldSize.y];
            
            // Process each tile to build node grid
            foreach (var map in tilemaps) {
                foreach (var pos in map.cellBounds.allPositionsWithin) {
                    // Get the grid position
                    Vector2Int gridPos = new Vector2Int(pos.x + offset.x, pos.y + offset.y);
                    
                    // Add to grid if missing
                    if (nodes[gridPos.x, gridPos.y] == null)
                        nodes[gridPos.x, gridPos.y] = new Node(gridPos, map.CompareTag(solidTag) && map.GetTile(pos) != null);
                    else if (map.CompareTag(solidTag) && map.GetTile(pos) != null)
                        nodes[gridPos.x, gridPos.y].Solid = true;
                }
            }
        }

        public void SetSolid(Vector3Int pos, bool solid) {
            Vector2Int gridPos = new Vector2Int(pos.x + offset.x, pos.y + offset.y);

            if (gridPos.x < 0 || gridPos.x > worldSize.x || gridPos.y < 0 || gridPos.y > worldSize.y)
                return;
            
            if (nodes[gridPos.x, gridPos.y] == null)
                nodes[gridPos.x, gridPos.y] = new Node(gridPos, solid);
            else nodes[gridPos.x, gridPos.y].Solid = solid;
        }
        
        public List<Node> GetNeighbours(Node node) {
            var neighbours = new List<Node>();

            for (var x = -1; x <= 1; x++) {
                for (var y = -1; y <= 1; y++) {
                    if ((x == 0 && y == 0)) continue;
                    AddNeighbour(node, x, y, ref neighbours);
                }
            }

            return neighbours;
        }

        private void AddNeighbour(Node node, int x, int y, ref List<Node> neighbours) {
            var checkX = node.GridPos.x + x;
            var checkY = node.GridPos.y + y;
            if (checkX >= 0 && checkX < worldSize.x && checkY >= 0 && checkY < worldSize.y) {
                neighbours.Add(nodes[checkX, checkY]);
            }
        }
    }
}