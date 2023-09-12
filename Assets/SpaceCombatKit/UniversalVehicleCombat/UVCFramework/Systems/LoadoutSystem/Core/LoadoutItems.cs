using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Loadout
{
    public class LoadoutItems : MonoBehaviour
    {
        [Tooltip("The vehicles available in the loadout.")]
        public List<LoadoutVehicleItem> vehicles = new List<LoadoutVehicleItem>();

        [Tooltip("The modules available in the loadout.")]
        public List<LoadoutModuleItem> modules = new List<LoadoutModuleItem>();
    }
}
