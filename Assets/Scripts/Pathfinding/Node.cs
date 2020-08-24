using UnityEngine;

namespace Pathfinding {
    public class Node {
        public Vector2Int GridPos;
        public bool Solid;
        public int G;
        public int H;

        public Node Parent;

        public int F {
            get { return G + H; }
        }

        public Node(Vector2Int pos, bool solid) {
            GridPos = pos;
            Solid = solid;
        }
    }
}