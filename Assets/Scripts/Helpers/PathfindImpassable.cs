using Pathfinding;
using UnityEngine;

namespace Helpers {
    public class PathfindImpassable : MonoBehaviour {
        private void Start() {
            var pos = NodeGrid.Instance.grid.WorldToCell(transform.position);
            NodeGrid.Instance.SetSolid(pos, true);
        }
    }
}