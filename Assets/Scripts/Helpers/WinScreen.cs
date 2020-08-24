using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Helpers {
    public class WinScreen : MonoBehaviour {
        public Text deathsText;
        public Text killsText;
        public Text collectedText;
        public Text finishedText;
        public Text lootText;
        
        private void Start() {
            var state = PlayerState.Instance;
            deathsText.text = "died " + state.deaths + " times";
            killsText.text = "killed " + state.kills + " enemies";
            collectedText.text = "collected " + state.collectedCompanions + " companions";
            finishedText.text = "fnished with " + state.finishedWithCompanions + " companions";
            lootText.text = "looted " + state.lootCount + " loot";
        }
        public void MainMenu() {
            SceneManager.LoadScene(0);
        }
    }
}