using Combat;
using Helpers;
using Pathfinding;
using UnityEngine;

namespace DefaultNamespace {
    public class BaseAI : MonoBehaviour {
        public bool debugPathfinding;
        public float moveSpeed;

        public GameObject bloodFX;
        
        protected Pathfinder Pathfinder;
        protected Rigidbody2D Rigidbody;
        protected Animator Animator;
        protected WeaponUser WeaponUser;

        protected void Start() {
            Pathfinder = GetComponent<Pathfinder>();
            Rigidbody = GetComponent<Rigidbody2D>();
            Animator = GetComponent<Animator>();
            WeaponUser = GetComponent<WeaponUser>();
        }

        public void Hit(float angle) {
            Instantiate(bloodFX, transform.position, Quaternion.Euler(new Vector3(angle - 90, -90, 0)));
        }

        protected bool MoveTo(Vector3 target) {
            if (GameManager.Instance.inDialogue) return true;
            var from = NodeGrid.Instance.grid.WorldToCell(transform.position);
            var to = NodeGrid.Instance.grid.WorldToCell(target);
            var path = Pathfinder.FindPath(new Vector2Int(from.x, from.y), new Vector2Int(to.x, to.y));

            // Move towards if path is available
            if (path.Count > 0) {
                // Debug render path
                if (debugPathfinding) {
                    var pos = transform.position;
                    foreach (var p in path) {
                        var d = new Vector3(p.x + 0.5f, p.y + 0.5f, 0) - pos;
                        Debug.DrawLine(pos, pos + d * 1, Color.blue);
                        pos += d * 1;
                    }
                }

                // Adds 0.5 offset to get middle of cell
                var direction = new Vector3(path[0].x + 0.5f, path[0].y + 0.5f, 0) - transform.position;
                direction.Normalize();

                Rigidbody.velocity = direction * moveSpeed;

                if (direction.magnitude > 0)
                    Animator.SetBool("Walking", true);
                else Animator.SetBool("Walking", false);
            
                return true;
            }
        
            Animator.SetBool("Walking", false);
            return false;
        }
        
        private bool _leftTriggered;
        private bool _rightTriggered = true;
        
        protected bool AnimationContoller(Vector3 target) {
            var flipped = false;
            if (target.x < transform.position.x) {
                if (!_leftTriggered) {
                    Animator.SetTrigger("FlipLeft");
                    _leftTriggered = true;
                    Animator.ResetTrigger("FlipRight");
                    _rightTriggered = false;
                }
                
                flipped = true;
            } else if (target.x > transform.position.x) {
                if (!_rightTriggered) {
                    Animator.SetTrigger("FlipRight");
                    _rightTriggered = true;
                    Animator.ResetTrigger("FlipLeft");
                    _leftTriggered = false;
                }
            }

            return flipped;
        }
    }
}