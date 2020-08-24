using Items;
using UnityEngine;

namespace Interaction {
    public class FoodPickup : Interactible {
        public Food food;

        private void Start() {
            base.OnInteract();
            hookDestroy = false;
            GetComponent<SpriteRenderer>().sprite = food.sprite;
            base.Start();
        }

        public void SetFood(Food food_) {
            food = food_;
            GetComponent<SpriteRenderer>().sprite = food.sprite;
        }

        public override bool OnInteract() {
            PlayerController.Instance.Heal(food.health);
            Destroy(gameObject);
            return true;
        }

        public override bool CanInteract() {
            return PlayerState.Instance.hitpoints < PlayerState.Instance.currentState.maxHP;
        }

        public override string GetInteractText() {
            return "eat " + food.displayName;
        }
    }
}