using System.Collections.Generic;
using Fog;
using Helpers;
using Interaction;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace.Boss {
    public class BossManager : MonoBehaviour {
        public BossConfig sequence;
        public float bulletRadius;

        public GameObject portableCrate;

        public SpriteRenderer renderer;

        public Bounds spawnBounds;

        public OpenDoor exitDoor;
        public OpenDoor entryDoor;
        
        public RoomFog roomFog;

        public Transform lootPosition;

        public AudioClip shoot;
        public AudioClip enrage;

        public bool bottomEntry;

        public GameObject bloodFX;

        private Animator _animator;
        private AudioSource _audioSource;

        private bool _dead;

        private bool _activated;
        
        private int _hitpoints;
        private int _currentPhase = -1;
        private bool _isTransitionPhase;

        private float _fireTimer;
        private float _endTimer;
        private float _phaseTimer;
        private float _lootCrateTimer;

        private float _hitTimer;
        private const float HitTime = 0.05f;

        private float _invulnerabilityTime;

        private int _rotationalOffset;

        private bool _spawnedEnemies;
        private List<GameObject> _enemies;

        private bool _readyToTransition;
        
        public void Hit(float angle) {
            Instantiate(bloodFX, transform.position, Quaternion.Euler(new Vector3(angle - 90, -90, 0)));
        }

        public void Damage(int damage) {
            if (_currentPhase == -1 || sequence.phases[_currentPhase].invulnerableDuringPhase || _readyToTransition || _dead)
                return;
            _hitpoints -= damage;
            
            _hitTimer = HitTime;
            renderer.material.SetInt("IsHit", 1);
            _animator.SetTrigger("Damaged");

            if (_hitpoints <= 0) {
                GameManager.Instance.hud.RemoveBoss();
                _animator.SetBool("Dead", true);
                _dead = true;
            } else {
                UpdateBossbar();
            }
        }

        private void Start() {
            _hitpoints = (int)(sequence.hitpoints * DifficultOwner.Instance.currentPreset.healthScale);
            _enemies = new List<GameObject>();
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void UpdateBossbar() {
            var ui = GameManager.Instance.hud;
            for (int i = 0; i < (int) Mathf.Ceil(sequence.hitpoints * DifficultOwner.Instance.currentPreset.healthScale / sequence.hitpointsPerHeart); i++) {
                if (_hitpoints >= i * sequence.hitpointsPerHeart + 2) {
                    ui.bossHearts[i].sprite = ui.heartFull;
                } else if (_hitpoints == i * sequence.hitpointsPerHeart + 1) {
                    ui.bossHearts[i].sprite = ui.heartHalf;
                } else ui.bossHearts[i].sprite = ui.heartEmpty;
            }
        }

        private void InitUI() {
            GameManager.Instance.hud.RegisterBoss(sequence.displayName, (int) Mathf.Ceil(sequence.hitpoints * DifficultOwner.Instance.currentPreset.healthScale / (float) sequence.hitpointsPerHeart));
            GameManager.Instance.hud.BossVulnerable();
            GameManager.Instance.InitBoss();
            UpdateBossbar();
            
        }

        private void Update() {
            if (_dead) {
                if (exitDoor.locked) {
                    var remove = new List<GameObject>();
                    foreach (var enemy in _enemies)
                        if (enemy == null)
                            remove.Add(enemy);

                    foreach (var enemy in remove)
                        _enemies.Remove(enemy);

                    if (_spawnedEnemies && _enemies.Count == 0) {
                        exitDoor.locked = false;
                        GameManager.Instance.BossEnd();
                        // Heal to full
                        PlayerController.Instance.Heal(26);
                        
                        // Spawn loot
                        var x = 0;
                        foreach (var loot in sequence.loot) {
                            var pos = lootPosition.position;
                            pos.x += x;
                            x += 1;

                            if (pos != Vector3.zero) {
                                Instantiate(loot, new Vector3(pos.x, pos.y, -0.2f), Quaternion.identity);
                            }
                        }
                    }
                    else if (!_spawnedEnemies) {
                        GameManager.Instance.BossEnd();
                        exitDoor.locked = false;
                        // Heal to full
                        PlayerController.Instance.Heal(26);
                        
                        // Spawn loot
                        var x = 0;
                        foreach (var loot in sequence.loot) {
                            var pos = lootPosition.position;
                            pos.x += x;
                            x += 1;

                            if (pos != Vector3.zero) {
                                Instantiate(loot, new Vector3(pos.x, pos.y, -0.2f), Quaternion.identity);
                            }
                        }
                    }
                }
                return;
            }
            if (_invulnerabilityTime > 0)
                _invulnerabilityTime -= Time.deltaTime;
            
            if (entryDoor != null) {
                if (!_activated &&
                    roomFog.visible &&
                    entryDoor.open &&
                    (bottomEntry ? PlayerController.Instance.transform.position.y > entryDoor.transform.position.y + 1 :
                                   PlayerController.Instance.transform.position.y < entryDoor.transform.position.y - 1)) {
                    entryDoor.open = false;
                    entryDoor.RefreshState();
                    entryDoor.locked = true;
                    exitDoor.locked = true;
                    _activated = true;
                    InitUI();
                    _audioSource.PlayOneShot(sequence.initiateEffect);
                }
            }

            if (!_activated) return;
            
            if (_hitTimer > 0)
                _hitTimer -= Time.deltaTime;
            else renderer.material.SetInt("IsHit", 0);

            _lootCrateTimer += Time.deltaTime;
            if (_readyToTransition || _currentPhase == -1)
                _phaseTimer += Time.deltaTime;
            
            // Transition timer
            if ((_readyToTransition || _currentPhase == -1) && _phaseTimer / sequence.phaseTransitionTime >= 1) {
                if (_currentPhase == -1) {
                    _currentPhase = 0;
                } else if (_currentPhase < sequence.phases.Count - 1) {
                    _currentPhase++;
                }
                
                _animator.SetTrigger("Transition");

                _spawnedEnemies = false;
                _readyToTransition = false;
                _phaseTimer = 0;
            }

            // Get current phase
            if (_currentPhase == -1 || _readyToTransition) {
                GameManager.Instance.hud.BossInvulnerable();
                return;
            }
            var phase = sequence.phases[_currentPhase];
            
            if (phase.invulnerableDuringPhase)
                GameManager.Instance.hud.BossInvulnerable();
            else GameManager.Instance.hud.BossVulnerable();

            // Check win condition
            switch (phase.endCondition) {
                case PhaseEndCondition.HitpointThreshold:
                    if (_hitpoints <= phase.hitpointThreshold * DifficultOwner.Instance.currentPreset.healthScale)
                        _readyToTransition = true;
                    break;
                case PhaseEndCondition.EnemiesKilled: {
                    var remove = new List<GameObject>();
                    foreach (var enemy in _enemies)
                        if (enemy == null)
                            remove.Add(enemy);

                    foreach (var enemy in remove)
                        _enemies.Remove(enemy);

                    if (_spawnedEnemies && _enemies.Count == 0) {
                        _readyToTransition = true;
                    }
                } break;
                case PhaseEndCondition.Timer:
                    _endTimer += Time.deltaTime;
                    if (_endTimer / phase.timerLength >= 1) {
                        _readyToTransition = true;
                        _endTimer = 0;
                    }

                    break;
                default: break;
            }

            // Phase actions
            if (phase.firesBullets) {
                _fireTimer += Time.deltaTime;
                if (_fireTimer / phase.timeBetweenShots >= 1) {
                    FireBullets(phase, _rotationalOffset, phase.bulletSpacing, phase.bulletVelocity);
                    _animator.SetTrigger("Shoot");
                    _audioSource.PlayOneShot(shoot);
                    _rotationalOffset += (int)(Random.Range(phase.rotationalOffsetIncrFrom, phase.rotationalOffsetIncrTo) * DifficultOwner.Instance.currentPreset.bulletScale);
                    _fireTimer = 0;
                }
            }

            if (!_spawnedEnemies && phase.spawnsEnemies) {
                _animator.SetTrigger("Summoning");
                int waveCost = 0;
                int iterations = 0;
                while (waveCost < phase.totalCost * DifficultOwner.Instance.currentPreset.costScale) {
                    if (iterations > 24) break;
                    iterations++;
                    
                    var enemyType = Random.Range(0, phase.possibleEnemies.Count);

                    var cost = phase.possibleEnemies[enemyType].GetComponent<EnemyAI>().enemyCost;
                    if (waveCost + cost <= phase.totalCost) {
                        Vector3 pos = Vector3.zero;
                        for (int attempt = 0; attempt < 10; attempt++) { // if this fails 10 times, player is lucky
                            var x = Random.Range(-spawnBounds.extents.x * 0.5f, spawnBounds.extents.x * 0.5f);
                            var y = Random.Range(-spawnBounds.extents.y * 0.5f, spawnBounds.extents.y * 0.5f);
                            pos = transform.position + spawnBounds.center + new Vector3(x, y, -0.3f);

                            var dist = pos - transform.position;
                            if (dist.magnitude < bulletRadius)
                                continue;
                            break;
                        }

                        if (pos != Vector3.zero) {
                            var enemy = Instantiate(phase.possibleEnemies[enemyType], pos, Quaternion.identity);
                            var ai = enemy.GetComponent<EnemyAI>();
                            ai.excludeFromCounter = true;
                            _enemies.Add(enemy);
                            waveCost += cost;
                        }
                    }
                }
                _spawnedEnemies = true;
            }

            if (_lootCrateTimer / sequence.lootCrateTimer >= 1) {
                Instantiate(portableCrate, PlayerController.Instance.transform.position, Quaternion.identity);
                _lootCrateTimer = 0;
            }
        }

        private void FireBullets(BossPhase phase, int rotationOffset, int spacing, float velocity) {
            for (var i = 0; i < 360; i += spacing) {
                var bulletPrefab = Random.Range(0, phase.bulletTypes.Count);
                var bullet = phase.bulletTypes[bulletPrefab];
                
                var a1 = Mathf.Cos((rotationOffset + i) * Mathf.Deg2Rad);
                var a2 = Mathf.Sin((rotationOffset + i) * Mathf.Deg2Rad);
                var x = bulletRadius * a1;
                var y = bulletRadius * a2;

                var bulletPos = transform.position + new Vector3(x, y, 0);
                var newBullet = Instantiate(bullet, bulletPos, Quaternion.identity);

                var dir = bulletPos - transform.position;
                
                var bulletComp = newBullet.GetComponent<Bullet>();
                bulletComp.goodBullet = false;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90 +
                              bulletComp.rotationOffset;
                newBullet.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
                newBullet.GetComponent<Rigidbody2D>().velocity = dir.normalized * velocity;
                Physics2D.IgnoreCollision(newBullet.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.DrawWireSphere(transform.position, bulletRadius);
            Gizmos.DrawWireCube(transform.position + spawnBounds.center, spawnBounds.extents);
        }
    }
}