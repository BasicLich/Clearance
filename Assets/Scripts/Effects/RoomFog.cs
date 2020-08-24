using System;
using System.Collections.Generic;
using Interaction;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fog {
    [RequireComponent(typeof(PolygonCollider2D)), RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class RoomFog : MonoBehaviour {
        public List<OpenDoor> doorTriggers;
        
        private Mesh _mesh;
        private Material _mat;

        public bool visible;
        private float _alpha = 0.99f;
        private const float AlphaRevealRate = 3f;

        private void Start() {
            var vertices2D = GetComponent<PolygonCollider2D>().points;
            
            // Use the triangulator to get indices for creating triangles
            Triangulator tr = new Triangulator(vertices2D);
            int[] indices = tr.Triangulate();
 
            // Create the Vector3 vertices
            Vector3[] vertices = new Vector3[vertices2D.Length];
            for (int i=0; i<vertices.Length; i++) {
                vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
            }
 
            // Create the mesh
            _mesh = new Mesh();
            _mesh.vertices = vertices;
            _mesh.triangles = indices;
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            
            // Add to mesh renderer
            GetComponent<MeshFilter>().mesh = _mesh;

            _mat = GetComponent<Renderer>().material;
        }

        private void Update() {
            if (_alpha != 0 && IsVisible()) {
                _alpha = Mathf.Lerp(_alpha, 0, AlphaRevealRate * Time.deltaTime);
                _mat.SetFloat("Alpha", _alpha);
            } else if (_alpha <= 0.99f && !IsVisible()) {
                _alpha = Mathf.Lerp(_alpha, 0.99f, AlphaRevealRate * Time.deltaTime);
                _mat.SetFloat("Alpha", _alpha);
            }
        }

        private void OnTriggerStay2D(Collider2D other) {
            if (other.gameObject.CompareTag("Player") && other is CapsuleCollider2D)
                visible = true;
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.CompareTag("Player"))
                visible = false;
        }

        private bool IsVisible() {
            if (doorTriggers.Count > 0) {
                foreach (var door in doorTriggers) {
                    if (door.open) return true;
                }
            }
            return visible;
        }
    }
}