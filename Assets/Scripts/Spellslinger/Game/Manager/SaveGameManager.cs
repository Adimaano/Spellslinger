namespace Spellslinger.Game.Manager
{
    using Spellslinger.Game.Control;
    using Spellslinger.Game.XR;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using UnityEngine;

    public class SaveGameManager : MonoBehaviour {
        // The name of the save file
        private static string saveFileName = "save.dat";
        private SaveData gameData;
        private static bool canSaveAndLoad = false;

        public static SaveGameManager Instance { get; private set; }

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(this.gameObject);
            }

            DontDestroyOnLoad(this.gameObject);

            // Check if the application has read and write access to the save file
            CheckReadWriteAccess();
            this.gameData = SaveGameManager.Load();
        }

        /// <summary>
        /// Saves the game data to the save file.
        /// </summary>
        /// <param name="saveData">The save data to save.</param>
        public static void Save(SaveData saveData) {
            if (!SaveGameManager.canSaveAndLoad) {
                // Use PlayerPrefs to save the data
                PlayerPrefs.SetInt("currentLevel", saveData.currentLevel);
                PlayerPrefs.SetInt("preferredHand", (int)saveData.preferredHand);
                PlayerPrefs.SetInt("availableSpellsCount", saveData.availableSpells.Count);

                for (int i = 0; i < saveData.availableSpells.Count; i++) {
                    PlayerPrefs.SetInt("availableSpells" + i, (int)saveData.availableSpells[i]);
                }
            } else {
                // Create a binary formatter
                BinaryFormatter formatter = new BinaryFormatter();

                // Create a file stream to write the save data
                FileStream stream = new FileStream(saveFileName, FileMode.Create);

                // Serialize the save data and write it to the file stream
                formatter.Serialize(stream, saveData);

                // Close the file stream
                stream.Close();
            }
        }

        /// <summary>
        /// Loads the save data from the save file.
        /// </summary>
        /// <returns>Returns SaveData of the savegame.</returns>
        public static SaveData Load() {
            if (!SaveGameManager.canSaveAndLoad) {
                // Use PlayerPrefs to load the data
                SaveData saveData = new SaveData();
                saveData.currentLevel = PlayerPrefs.GetInt("currentLevel", 1);
                saveData.preferredHand = (XRInputManager.Controller)PlayerPrefs.GetInt("preferredHand", 0);
                int availableSpellsCount = PlayerPrefs.GetInt("availableSpellsCount", 0);

                for (int i = 0; i < availableSpellsCount; i++) {
                    saveData.availableSpells.Add((SpellCasting.Spell)PlayerPrefs.GetInt("availableSpells" + i, 0));
                }

                return saveData;
            } else {
                // Check if the save file exists
                if (File.Exists(saveFileName)) {
                    // Create a binary formatter
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Create a file stream to read the save data
                    FileStream stream = new FileStream(saveFileName, FileMode.Open);

                    // Deserialize the save data from the file stream
                    SaveData saveData = formatter.Deserialize(stream) as SaveData;

                    // Close the file stream
                    stream.Close();

                    // Return the save data
                    return saveData;
                } else {
                    // Return a new instance of the save data if the save file does not exist
                    return new SaveData();
                }
            }
        }

        private static void CheckReadWriteAccess() {
            // Check if the application has read and write access to the save file, set the canSaveAndLoad flag accordingly
            SaveGameManager.canSaveAndLoad = true;

            try {
                // Create a file stream to check if the application has read and write access to the save file
                FileStream stream = new FileStream(saveFileName, FileMode.OpenOrCreate);

                // Close the file stream
                stream.Close();
            } catch (System.Exception) {
                SaveGameManager.canSaveAndLoad = false;
            }
        }

        /// <summary>
        /// Returns the loaded save data.
        /// </summary>
        /// <returns>The loaded save data.</returns>
        public SaveData GetSaveData() {
            return this.gameData;
        }

        /// <summary>
        /// Returns wether a save file exists.
        /// </summary>
        /// <returns>True if a save file exists, false otherwise.</returns>
        public static bool SaveFileExists() {
            return File.Exists(saveFileName);
        }
    }
}