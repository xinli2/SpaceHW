using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Save/load loadout data to a JSON file.
    /// </summary>
    public class LoadoutDataManagerJSON : LoadoutDataManager
    {
        [Tooltip("The file name for the saved loadout data.")]
        [SerializeField]
        protected string fileName = "SavedLoadout";

        [Tooltip("Whether to debug saving/loading in the console.")]
        [SerializeField]
        protected bool debug = false;

        /// <summary>
        /// Save the loadout data.
        /// </summary>
        /// <param name="data">The loadout data.</param>
        public override void SaveData(LoadoutData data)
        {
            string dataString = JsonUtility.ToJson(data);

            File.WriteAllText(GetFilePath(), dataString);

            if (debug)
            {
                Debug.Log("Saved loadout to file at path " + GetFilePath());
                Debug.Log("Saved data: " + dataString);
            }
        }

        /// <summary>
        /// Load the loadout data.
        /// </summary>
        /// <returns>The loadout data.</returns>
        public override LoadoutData LoadData()
        {
            if (!File.Exists(GetFilePath()))
            {
                if (debug) Debug.Log("Load failed, file at " + GetFilePath() + " does not exist.");
                return null;
            }

            string dataString = File.ReadAllText(GetFilePath());

            LoadoutData data = JsonUtility.FromJson<LoadoutData>(dataString);

            if (debug)
            {
                Debug.Log("Loaded file at path " + GetFilePath());
                Debug.Log("Loaded data: " + dataString);
            }

            return data;
        }


        /// <summary>
        /// Get the file path for the save/load.
        /// </summary>
        /// <returns>The save file path.</returns>
        public virtual string GetFilePath()
        {
            return (Application.persistentDataPath + "/" + fileName + ".json");
        }


        /// <summary>
        /// Delete the save data.
        /// </summary>
        public override void DeleteSaveData()
        {
            bool exists = File.Exists(GetFilePath());

            File.Delete(GetFilePath());

            if (debug)
            {
                if (exists)
                    Debug.Log("Deleted file at path " + GetFilePath());
                else
                    Debug.Log("Delete failed, file not found at path " + GetFilePath());
            }
        }
    }
}
