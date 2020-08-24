using System;
using DefaultNamespace;
using DefaultNamespace.Boss;
using Interaction;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public float lifetime;
    public float rotationOffset;
    public int damage;
    public bool explosive;
    // public GameObject fireEffect;

    public Material badMat;
    private bool _doneBad;

    public bool goodBullet;

    private float timer;
    private BoxCollider2D _box;
    private bool _hit;

    private void Start() {
        _box = GetComponent<BoxCollider2D>();
        
        if (!_doneBad && !goodBullet) {
            GetComponent<SpriteRenderer>().material = badMat;
            _doneBad = true;
        }
    }
    
    private void Update() {
        if (!_doneBad && !goodBullet) {
            GetComponent<SpriteRenderer>().material = badMat;
            _doneBad = true;
        }
        
        timer += Time.deltaTime;

        if (timer >= lifetime) {
            Destroy(gameObject);
            if (explosive) Explode();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other) {
        if ((other.gameObject.CompareTag("Solid") || other.gameObject.CompareTag("Interactible")) && !_hit) {
            OnHit();
            return;
        }

        if (goodBullet) {
            if (other.gameObject.CompareTag("Enemy") && !_hit) {
                // Damage enemy
                var enemy = other.gameObject.GetComponent<EnemyAI>();
                enemy.Damage(damage);
                enemy.Hit(transform.localRotation.eulerAngles.z - rotationOffset);
                OnHit();
            } else if (other.gameObject.CompareTag("Explosive")) {
                // Trigger explosion
                other.gameObject.GetComponent<Explosive>().Explode();
                OnHit();
            } else if (other.gameObject.CompareTag("Boss")) {
                var boss = other.gameObject.GetComponent<BossManager>();
                boss.Damage(damage);
                boss.Hit(transform.localRotation.eulerAngles.z - rotationOffset);
                OnHit();
            } else if (other.gameObject.CompareTag("Companion")) {
                OnHit();
            }
        } else {
            if (other.gameObject.CompareTag("Player") && !_hit) {
                // Damage player
                var player = other.gameObject.GetComponent<PlayerController>();
                player.Damage(damage);
                player.Hit(transform.localRotation.eulerAngles.z - rotationOffset);
                OnHit();
            } else if (other.gameObject.CompareTag("Companion") && !_hit) {
                // Damage player
                var companion = other.gameObject.GetComponent<CompanionAI>();
                companion.Damage(damage);
                companion.Hit(transform.localRotation.z - rotationOffset);
                OnHit();
            } else if (other.gameObject.CompareTag("Explosive")) {
                // Trigger explosion
                other.gameObject.GetComponent<Explosive>().Explode();
                OnHit();
            } else if (other.gameObject.CompareTag("Enemy") && !_hit) {
                OnHit();
            }
        }
    }

    private void OnHit() {
        // Destroy bullet
        Destroy(gameObject);
            
        // Mark as hit
        _hit = true;
            
        // Explode!!
        if (explosive) Explode();
    }

    private void Explode() {
        GetComponent<Explosive>().Explode();
    }
}