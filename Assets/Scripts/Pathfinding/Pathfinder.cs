using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding {
    public class Pathfinder : MonoBehaviour {

        public Vector3Int WorldToCell(Vector3 pos) {
            return NodeGrid.Instance.grid.WorldToCell(pos);
        }
        
        public List<Vector3Int> FindPath(Vector2Int from, Vector2Int to) {
            if (from.x + NodeGrid.Instance.offset.x < 0 || from.y + NodeGrid.Instance.offset.y < 0 || to.x + NodeGrid.Instance.offset.x < 0 || to.y + NodeGrid.Instance.offset.y < 0)
                return new List<Vector3Int>();
            if (from.x + NodeGrid.Instance.offset.x > NodeGrid.Instance.nodes.GetLength(0) ||
                from.y + NodeGrid.Instance.offset.y > NodeGrid.Instance.nodes.GetLength(1) ||
                to.x + NodeGrid.Instance.offset.x > NodeGrid.Instance.nodes.GetLength(0) ||
                to.y + NodeGrid.Instance.offset.y > NodeGrid.Instance.nodes.GetLength(1))
                return new List<Vector3Int>();

            var startNode = NodeGrid.Instance.nodes[from.x + NodeGrid.Instance.offset.x, from.y + NodeGrid.Instance.offset.y];
            var targetNode = NodeGrid.Instance.nodes[to.x + NodeGrid.Instance.offset.x, to.y + NodeGrid.Instance.offset.y];
            // if (startNode == null || startNode.Solid || targetNode == null || targetNode.Solid)
            if (startNode == null || targetNode == null || targetNode.Solid)
                return new List<Vector3Int>();

            var openSet = new List<Node>();
            var closedSet = new HashSet<Node>();
            
            // Add original position to open list
            openSet.Add(startNode);

            int iterations = 0;

            while (openSet.Count > 0) {
                // Stop bad AI looping
                if (iterations > 16)
                    return new List<Vector3Int>();
                iterations++;
                
                Node node = openSet[0];

                for (var i = 1; i < openSet.Count; i++) {
                    if (openSet[i].F < node.F || openSet[i].F == node.F)
                        if (openSet[i].H < node.H)
                            node = openSet[i];
                }

                openSet.Remove(node);
                closedSet.Add(node);

                if (node == targetNode) {
                    return NodePathToIntPath(RetracePath(startNode, targetNode));
                }

                // Calculate costs and stuff
                foreach (var neighbour in NodeGrid.Instance.GetNeighbours(node)) {
                    if (neighbour.Solid || closedSet.Contains(neighbour))
                        continue;

                    int newCostToNeighbour = node.G + GetDistance(node, neighbour);
                    if (newCostToNeighbour < neighbour.G || !openSet.Contains(neighbour)) {
                        neighbour.G = newCostToNeighbour;
                        neighbour.H = GetDistance(neighbour, targetNode);
                        neighbour.Parent = node;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }

            return NodePathToIntPath(RetracePath(startNode, targetNode));
        }

        private List<Node> RetracePath(Node startNode, Node endNode) {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            int depth = 0;

            while (currentNode != startNode) {
                depth++;
                if (depth > 32) return new List<Node>();
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        private List<Vector3Int> NodePathToIntPath(List<Node> nodePath) {
            var path = new List<Vector3Int>();
            foreach (var node in nodePath)
                path.Add(new Vector3Int(node.GridPos.x - NodeGrid.Instance.offset.x, node.GridPos.y - NodeGrid.Instance.offset.y, 0));
            return path;
        }

        // Don't really understand the multiplications, but I don't got time to argue
        private int GetDistance(Node a, Node b) {
            int dstX = Mathf.Abs(a.GridPos.x - b.GridPos.x);
            int dstY = Mathf.Abs(a.GridPos.y - b.GridPos.y);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}