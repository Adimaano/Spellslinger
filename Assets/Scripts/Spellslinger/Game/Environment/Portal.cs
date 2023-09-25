namespace Spellslinger.Game.Environment
{
    using Spellslinger.Game.Environment;
    using Spellslinger.Game.Manager;
    using UnityEngine;

    public class Portal : MonoBehaviour {
        [SerializeField] private Material portalMaterial;
        [SerializeField] private Material portalMaterialDefault;
        private bool isActive = false;
        private int levelToLoad = 0;

        public int LevelToLoad { get => this.levelToLoad; set => this.levelToLoad = value; }
        public bool IsActive { get => this.isActive; set => this.isActive = value; }

        private void OnTriggerEnter(Collider other) {
            if (this.IsActive && other.CompareTag("Player")) {
                // Set the final intensity value
                Color baseEmissionColor = this.portalMaterialDefault.GetColor("_EmissionColor");
                this.portalMaterial.SetColor("_EmissionColor", baseEmissionColor);

                if (this.LevelToLoad == 0) {
                    // Get next level
                    this.LevelToLoad = GameManager.Instance.GetNextLevel();
                }

                // Update and save Levelprogress in Savegame
                SaveData saveData = SaveGameManager.Instance.GetSaveData();
                saveData.currentLevel = this.LevelToLoad;
                SaveGameManager.Save(saveData);

                GameManager.Instance.LoadLevel(this.LevelToLoad);
            }
        }
    }
}