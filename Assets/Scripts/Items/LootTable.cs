using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Items {
    [Serializable]
    public class FoodLootItem {
        public Food food;
        public float weight;

        [HideInInspector]
        public float rangeFrom;
        [HideInInspector]
        public float rangeTo;
    }
    
    [Serializable]
    public class WeaponLootItem {
        public Weapon weapon;
        public float weight;

        [HideInInspector]
        public float rangeFrom;
        [HideInInspector]
        public float rangeTo;
    }
    
    [CreateAssetMenu(fileName = "loot.asset", menuName = "Items/Loot Table", order = 0)]
    public class LootTable : ScriptableObject {
        public List<FoodLootItem> foods;
        public List<WeaponLootItem> weapons;

        // % chance for a drop when the game is dropped
        public float weaponChance;
        public float foodChance;
        public float ammoChance;
        
        [NonSerialized]
        private float _foodWeight;
        
        [NonSerialized]
        private float _weaponWeight;

        [NonSerialized]
        private bool _validated;

        public void Get(out Food food, out Weapon weapon, out bool ammo) {
            var n = UnityEngine.Random.Range(0, 100);
            if (n < weaponChance)
                weapon = PickWeapon();
            else weapon = null;
            
            n = UnityEngine.Random.Range(0, 100);
            if (n < foodChance)
                food = PickFood();
            else food = null;

            n = UnityEngine.Random.Range(0, 100);
            ammo = n < ammoChance;
        }

        public void ValidateTable() {
            if (_validated) return;
            _validated = true;
            if (foods != null && foods.Count > 0) {
                float currentWeightMaximum = 0f;

                foreach (var item in foods) {
                    if (item.weight < 0f) item.weight = 0f;
                    item.rangeFrom = currentWeightMaximum;
                    currentWeightMaximum += item.weight;
                    item.rangeTo = currentWeightMaximum;
                }

                _foodWeight = currentWeightMaximum;
            }
            
            if (weapons != null && weapons.Count > 0) {
                float currentWeightMaximum = 0f;

                foreach (var item in weapons) {
                    if (item.weight < 0f) item.weight = 0f;
                    item.rangeFrom = currentWeightMaximum;
                    currentWeightMaximum += item.weight;
                    item.rangeTo = currentWeightMaximum;
                }

                _weaponWeight = currentWeightMaximum;
            }
        }
        
        public Food PickFood() {
            float n = UnityEngine.Random.Range(0, _foodWeight);
            foreach (var item in foods) {
                if (n > item.rangeFrom && n < item.rangeTo)
                    return item.food;
            }
            return foods[0].food;
        }

        public Weapon PickWeapon() {
            float n = UnityEngine.Random.Range(0, _weaponWeight);
            foreach (var item in weapons) {
                if (n > item.rangeFrom && n < item.rangeTo)
                    return item.weapon;
            }
            return weapons[0].weapon;
        }
    }
}