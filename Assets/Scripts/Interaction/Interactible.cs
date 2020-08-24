using Helpers;
using UnityEngine;

namespace Interaction {
    public class Interactible : Destructible {
        public Material highlight;
        public Material defaultMaterial;
        public SpriteRenderer spriteRenderer;
        
        public AudioClip useSound;
        
        protected void Start() {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            base.Start();
        }
        
        public virtual bool OnInteract() {
            if (useSound != null)
                GameManager.Instance.audioSource.PlayOneShot(useSound);
            return false;
        }

        public virtual bool CanInteract() {
            return true;
        }

        public virtual string GetInteractText() {
            return "Interact";
        }

        public void Outline() {
            if (spriteRenderer != null)
                spriteRenderer.material = highlight;
        }

        public void StopOutline() {
            if (spriteRenderer != null)
                spriteRenderer.material = defaultMaterial;
        }
    }
}