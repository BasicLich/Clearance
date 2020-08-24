using System.Collections.Generic;
using UnityEngine;

namespace Interaction {
    public class Rescue : NPC {
        public GameObject companionPrefab;

        private void Start() {
            base.Start();
            if (PlayerState.Instance.savedOverall != null && PlayerState.Instance.savedOverall.Contains(new SavedCompanion(transform.position, companionPrefab))) {
                Destroy(gameObject);
            }
        }
        
        public override bool OnInteract() {
            if (PlayerState.Instance.savedThisLife == null)
                PlayerState.Instance.savedThisLife = new List<SavedCompanion>();
            PlayerState.Instance.savedThisLife.Add(new SavedCompanion(transform.position, companionPrefab));
            var companion = Instantiate(companionPrefab, transform.position, Quaternion.identity);
            companion.GetComponent<CompanionAI>().myPrefab = companionPrefab;
            Destroy(gameObject);
            PlayerState.Instance.collectedCompanions++;
            return base.OnInteract();
        }

        public override string GetInteractText() {
            return "rescue";
        }
    }
}