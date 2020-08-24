using System.Collections;
using System.Collections.Generic;
using Helpers;
using Pathfinding;
using UnityEngine;

namespace Interaction {
    public class OpenDoor : Interactible {
        public Sprite openSprite;
        public bool open;
        public bool locked;
        public bool dependOnObjectives;

        public bool nextLevel;

        public bool pathfindUpdate = true;

        private SpriteRenderer _sRenderer;
        private BoxCollider2D _collider;
        private Sprite _originalSprite;

        private Vector3Int _gridPosition;

        private void Start() {
            hookDestroy = false;
            base.Start();
            _sRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            _originalSprite = _sRenderer.sprite;
            _gridPosition = NodeGrid.Instance.grid.WorldToCell(transform.position);
            NodeGrid.Instance.SetSolid(_gridPosition, true);
        }

        public void RefreshState() {
            if (open) {
                _sRenderer.sprite = openSprite;
                _collider.isTrigger = true;
                if (pathfindUpdate)
                    NodeGrid.Instance.SetSolid(_gridPosition, false);
            } else {
                _sRenderer.sprite = _originalSprite;
                _collider.isTrigger = false;
                if (pathfindUpdate)
                    NodeGrid.Instance.SetSolid(_gridPosition, true);
            }
        }
        
        public override bool OnInteract() {
            if (locked) return false;
            if (dependOnObjectives && !GameManager.Instance.ObjectivesCompleted()) return false;
            base.OnInteract();

            if (nextLevel) {
                // Collect and save companions
                StartCoroutine(LevelTransition());
            }
            
            open = !open;
            RefreshState();
            return false;
        }

        private IEnumerator LevelTransition() {
            var mgr = GameManager.Instance;

            var color = mgr.transitionOverlay.color;
            color.a = 0f;
            mgr.transitionOverlay.color = color;
            
            while (color.a < 1f) {
                color.a += 0.05f;
                mgr.transitionOverlay.color = color;
                yield return new WaitForSeconds(0.05f);
            }
            
            mgr.NextLevel();
        }

        public override string GetInteractText() {
            return locked ? "locked" : (dependOnObjectives && !GameManager.Instance.ObjectivesCompleted()) ? "incomplete tasks" : open ? "close" : "open";
        }
    }
}