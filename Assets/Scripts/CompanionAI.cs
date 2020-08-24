using System;
using Combat;
using DefaultNamespace;
using Helpers;
using Pathfinding;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = System.Random;

[RequireComponent(typeof(Pathfinder)), RequireComponent(typeof(WeaponUser)), RequireComponent(typeof(Rigidbody2D))]
public class CompanionAI : BaseAI {
    public LayerMask solidMask;
    public LayerMask playerMask;
    public int hitpoints;

    public GameObject myPrefab;

    public SpriteRenderer weaponRenderer;

    private Vector3 _initialPosition;
    
    private void Start() {
        base.Start();
        WeaponUser.timeoutMultiplier = 1.5f;
        weaponRenderer.sprite = WeaponUser.primary.sprite;
        _initialPosition = transform.position;

        if (PlayerState.Instance.companionsDiedOverall.Contains(_initialPosition))
            Destroy(gameObject);
        
        transform.position = new Vector3(transform.position.x, transform.position.y, -0.2f);
    }

    private Collider2D _currentEnemy;
    private bool _hasEnemy;
    private bool _targetingEnemy;

    private bool _patrol;
    private Vector3 _patrolTarget;

    public GameObject targetEnemy;

    public void Damage(int damage) {
        hitpoints -= damage;
        if (hitpoints < 0 && SceneManager.GetActiveScene().buildIndex != 0) {
            PlayerState.Instance.companionsDiedThisLife.Add(_initialPosition);
            Destroy(gameObject);
        }
    }

    private void FixedUpdate() {
        // Get player
        var playerController = PlayerController.Instance;

        // Look for hostiles
        if (!_hasEnemy) {
            var colliders = Physics2D.OverlapCircleAll(transform.position, 5f);
            float smallestDistance = Single.MaxValue;
            foreach (var target in colliders) {
                if (target.CompareTag("Enemy")) {
                    var dist = (target.transform.position - transform.position).magnitude;
                    if (dist < smallestDistance) {
                        smallestDistance = dist;
                        _currentEnemy = target;
                        _hasEnemy = true;
                        targetEnemy = target.gameObject;
                    }
                }
            }
        } else if (_currentEnemy == null) {
            _hasEnemy = false;
            targetEnemy = null;
        }

        if (_hasEnemy) {
            _patrol = false;
            
            // Line of sight
            var dir = _currentEnemy.transform.position - transform.position;
            var check = Physics2D.Raycast(transform.position, dir.normalized, dir.magnitude, solidMask);

            if (check.collider == null) {
                WeaponUser.PointGun(_currentEnemy.transform.position, AnimationContoller(_currentEnemy.transform.position));

                if (dir.magnitude < 10f) {
                    // Check the player isnt in the way
                    var playerCheck = Physics2D.Raycast(transform.position, dir.normalized, dir.magnitude, playerMask);
                    
                    if (playerCheck.collider == null && WeaponUser.FirePrimary(Rigidbody, transform.position, dir.normalized)) {
                        weaponRenderer.sprite = WeaponUser.primary.sprite;
                    }
                    Animator.SetBool("Walking", false);
                } else {
                    MoveTo(_currentEnemy.transform.position);
                    Animator.SetBool("Walking", true);
                }
            } else {
                _hasEnemy = false;
                targetEnemy = null;
            }
        }

        if (!_hasEnemy) {
            var playerState = PlayerState.Instance;

            if (playerState.companionFollow) {
                _patrol = false;
                var dist = (playerController.transform.position - transform.position).magnitude;
                AnimationContoller(playerController.transform.position);
                WeaponUser.StopPointGun();
                if (dist > 1.5f) {
                    MoveTo(playerController.transform.position);
                    Animator.SetBool("Walking", true);
                } else Animator.SetBool("Walking", false);
            } else if (_patrol) {
                _patrol = MoveTo(_patrolTarget);
                AnimationContoller(_patrolTarget);
                WeaponUser.StopPointGun();
                Animator.SetBool("Walking", true);
            } else {
                // Pick random destination
                _patrolTarget = playerController.transform.position;
                _patrolTarget.x += UnityEngine.Random.Range(-3, 3);
                _patrolTarget.y += UnityEngine.Random.Range(-3, 3);
                _patrol = MoveTo(_patrolTarget);
                AnimationContoller(_patrolTarget);
                WeaponUser.StopPointGun();
                Animator.SetBool("Walking", true);
            }
            
            // TP if far away
            if (!GameManager.Instance.bossBattle) {
                var d = (playerController.transform.position - transform.position).magnitude;
                if (d > 15)
                    transform.position = playerController.transform.position;
            }
        }
    }

    
}