using System;
using Combat;
using DefaultNamespace;
using Helpers;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Pathfinder)), RequireComponent(typeof(Animator))]
public class EnemyAI : BaseAI {
    public int wanderChance;
    public float attackRange;
    public LayerMask solidLayer;

    public int hitpoints;

    public bool excludeFromCounter;

    public int enemyCost;
    
    private WeaponUser _weaponUser;
    private bool _hasWeapon;

    // TODO: Rework if we support companions...
    private Transform _target;
    private bool _seesTarget;
    private Vector2 _lastKnownLocation;
    private bool _seenTarget;

    private float _wanderTimer;
    private bool _wandering;
    private Vector2 _wanderTarget;

    private bool _canMove;

    private const float HitTime = 0.05f; // seconds
    private float _hitTimer;
    private bool _red;

    private Vector3 _initialPosition;

    private bool _addedToList;

    public void Damage(int damage) {
        hitpoints -= damage;
        if (hitpoints <= 0 && SceneManager.GetActiveScene().buildIndex != 0) {
            if (!excludeFromCounter && !_addedToList) {
                PlayerState.Instance.killedThisLife.Add(_initialPosition);
                _addedToList = true;
            }

            PlayerState.Instance.kills++;
            Destroy(gameObject);
        }

        _hitTimer = HitTime;
        GetComponentInChildren<Renderer>().material.SetInt("IsHit", 1);
    }

    private void Start() {
        _weaponUser = GetComponent<WeaponUser>();
        _hasWeapon = _weaponUser != null;

        _initialPosition = transform.position;

        hitpoints = (int) (hitpoints * DifficultOwner.Instance.currentPreset.healthScale);

        base.Start();
        
        if (!excludeFromCounter && PlayerState.Instance.killedOverall.Contains(_initialPosition))
            Destroy(gameObject);
    }

    private Vector3 _currentTarget;
    private bool _hasTarget;
    private bool _targetIsHostile;
    private bool _targetIsPlayer;
    private Collider2D _targetHostile;
    private bool _isWandering;

    private Vector3 _lastPos;
    private float _stuckTimer;

    private void FixedUpdate() {
        // Flash red if damaged
        if (_hitTimer > 0)
            _hitTimer -= Time.deltaTime;
        else GetComponentInChildren<Renderer>().material.SetInt("IsHit", 0);
        
        // Stuck checker
        if (_hasTarget && transform.position == _lastPos)
            _stuckTimer += Time.deltaTime;
        else _stuckTimer = 0;
        _lastPos = transform.position;

        // ===========
        // NEW ENEMY AI
        // ===========
        
        if (_hasTarget && _stuckTimer > 4f)
            _hasTarget = false;

        // Get an enemy target if we haven't already got a target
        if (!_hasTarget || _isWandering || !_targetIsPlayer) {
            // Check if we can see a target. (be it player or companion)
            var targets = Physics2D.OverlapCircleAll(transform.position, 5); // TODO: Variable for range
            
            var smallestDist = Single.MaxValue;
            foreach (var target in targets) {
                var dir = target.transform.position - transform.position;
                var check = Physics2D.Raycast(transform.position, dir, dir.magnitude, solidLayer);

                if (check.collider == null) {
                    if (target.CompareTag("Player")) {
                        var dist = (target.transform.position - transform.position).magnitude;
                        if (dist < smallestDist) {
                            smallestDist = dist;
                            _currentTarget = target.transform.position;
                            _targetIsHostile = true;
                            _hasTarget = true;
                            _targetHostile = target;
                            _isWandering = false;
                            _targetIsPlayer = true;
                        }
                    }
                    if (target.CompareTag("Companion")) {
                        var dist = (target.transform.position - transform.position).magnitude;
                        var companionAI = target.GetComponent<CompanionAI>();
                        if (dist < smallestDist && companionAI.hitpoints > 0) {
                            smallestDist = dist;
                            _currentTarget = target.transform.position;
                            _targetIsHostile = true;
                            _hasTarget = true;
                            _targetHostile = target;
                            _isWandering = false;
                            _targetIsPlayer = false;
                        }
                        
                        if (companionAI.targetEnemy == gameObject) {
                            _currentTarget = target.transform.position;
                            _targetIsHostile = true;
                            _hasTarget = true;
                            _targetHostile = target;
                            _isWandering = false;
                            _targetIsPlayer = false;
                            break;
                        }
                    }
                }
            }
        } else if (_targetHostile == null) {
            _hasTarget = false;
            _targetIsPlayer = false;
        }

        // Get direction vector
        var targetDir = _currentTarget - transform.position;

        if (_hasTarget) {
            if (targetDir.magnitude > 10) {
                _hasTarget = false;
                _targetIsPlayer = false;
            }
        }

        if (_hasTarget) {
            // Check if we have line of sight of the target (if we have one)
            if (_targetIsHostile) {
                // If we can't see the target, we forget him
                var check = Physics2D.Raycast(transform.position, targetDir, targetDir.magnitude, solidLayer);

                if (check.collider == null) {
                    WeaponUser.PointGun(_targetHostile.transform.position, AnimationContoller(_targetHostile.transform.position));
                    
                    // Update target
                    _currentTarget = _targetHostile.transform.position;
                    targetDir = _currentTarget - transform.position;
                    
                    if (_hasWeapon && targetDir.magnitude < attackRange) {
                        _weaponUser.FirePrimary(Rigidbody, transform.position, targetDir.normalized);
                        return;
                    }
                } else {
                    _hasTarget = false;
                    _targetIsHostile = false;
                    _isWandering = false;
                    _targetIsPlayer = false;
                }
                
                Animator.SetBool("Walking", _hasTarget);
            } else if (targetDir.magnitude < 0.5f) {
                _hasTarget = false;
                _targetIsHostile = false;
                _isWandering = false;
                _targetIsPlayer = false;
            }
            
            // Move towards target
            _hasTarget = MoveTo(_currentTarget);
            AnimationContoller(_currentTarget);
            Animator.SetBool("Walking", _hasTarget);
        } else {
            _wanderTimer += Time.deltaTime;
            var r = new System.Random();
            if (_wanderTimer >= 1 && r.Next(0, 100) <= wanderChance) {
                // wanderChance% chance to move
                // Select a random cell within 2 positions, we have 4 tries before failing
                for (var i = 0; i < 4; i++) {
                    var x = r.Next(-2, 2);
                    var y = r.Next(-2, 2);
                
                    _currentTarget = transform.position + new Vector3(x, y, 0);
                    _hasTarget = MoveTo(_currentTarget);
                    _isWandering = _hasTarget;
                    Animator.SetBool("Walking", _hasTarget);
                    AnimationContoller(_currentTarget);
                    if (_hasTarget) break;
                }
                
                // Reset timer
                _wanderTimer = 0;
            } else {
                Animator.SetBool("Walking", false);
            }
        }
    }
}