using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// A loadout slot in the loadout menu - represents a vehicle with a specific loadout.
    /// </summary>
    [System.Serializable]
    public class LoadoutSlot
    {
        [Tooltip("The selected vehicle index (within an associated Loadout Items component's Vehicles list).")]
        public int selectedVehicleIndex;

        [Tooltip("The selected module indexes (within an associated Loadout Items component's Modules list) for each module mount.")]
        public List<int> selectedModules = new List<int>();
    }
}
