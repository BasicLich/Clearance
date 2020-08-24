using UnityEngine;

namespace Interaction {
    public class Explosive : MonoBehaviour {
        public float explodeTimeout;
        public float explosionRadius;
        public int explosionDamage;

        public GameObject smokeEffect;

        public bool changeSpriteOnIgnite;
        public Sprite igniteSprite;

        public bool emitLightOnIgnite;
        public GameObject lightEmitter;

        private bool _ignited;
        private float _explosionTimer;
        private bool _exploded;

        public void Explode() {
            if (_ignited) return;
            
            if (changeSpriteOnIgnite)
                GetComponent<SpriteRenderer>().sprite = igniteSprite;

            if (emitLightOnIgnite)
                Instantiate(lightEmitter, transform);

            if (explodeTimeout == 0)
                DoExplode();
            else {
                _explosionTimer = explodeTimeout;
                _ignited = true;
            }
        }

        private void Update() {
            if (_ignited) {
                if (_explosionTimer > 0)
                    _explosionTimer -= Time.deltaTime;
                else DoExplode();
            }
        }

        public void DoExplode() {
            if (_exploded) return;
            _exploded = true;
            var colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (var other in colliders) {
                if (!other.isTrigger && (transform.position - other.transform.position).magnitude <= explosionRadius) {
                    if (other.gameObject.CompareTag("Enemy")) {
                        other.gameObject.GetComponent<EnemyAI>().Damage(explosionDamage);
                    } else if (other.gameObject.CompareTag("Player")) {
                        other.gameObject.GetComponent<PlayerController>().Damage(explosionDamage);
                    } else if (other.gameObject.CompareTag("Explosive")) {
                        other.gameObject.GetComponent<Explosive>().DoExplode();
                        continue;
                    } else if (other.gameObject.CompareTag("Companion")) {
                        other.gameObject.GetComponent<CompanionAI>().Damage(explosionDamage);
                    }

                    // If it dont work, i dont care
                    if (other.attachedRigidbody != null) {
                        var dir = other.transform.position - transform.position;
                        dir.Normalize();
                        other.attachedRigidbody.AddForce(dir * 15f, ForceMode2D.Impulse);
                    }
                }
            }

            // Spawn particles
            Instantiate(smokeEffect, transform.position, Quaternion.identity);
                    
            Destroy(gameObject);
        }
        
        private void OnDrawGizmosSelected() {
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}