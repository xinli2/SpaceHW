using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Base class for a component that saves and loads loadout information.
    /// </summary>
    public class LoadoutDataManager : MonoBehaviour
    {
        /// <summary>
        /// Save the loadout data.
        /// </summary>
        /// <param name="data">The loadout data.</param>
        public virtual void SaveData(LoadoutData data) { }

        /// <summary>
        /// Load the loadout data.
        /// </summary>
        /// <returns>The loadout data.</returns>
        public virtual LoadoutData LoadData()
        {
            return null;
        }

        /// <summary>
        /// Delete the saved loadout data.
        /// </summary>
        public virtual void DeleteSaveData() { }
    }
}
