using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Display info for a loadout vehicle.
    /// </summary>
    public class LoadoutVehicleInfoController : LoadoutItemInfoController
    {
        /// <summary>
        /// Update the loadout vehicle info.
        /// </summary>
        public override void UpdateInfo()
        {

            base.UpdateInfo();

            if (loadoutManager.Items == null) { Hide(); return; }

            if (loadoutManager.WorkingSlot.selectedVehicleIndex == -1) { Hide(); return; }

            LoadoutVehicleItem vehicleItem = loadoutManager.Items.vehicles[loadoutManager.WorkingSlot.selectedVehicleIndex];

            foreach (LoadoutItemInfoOverrideController overrideController in overrideControllers)
            {
                if (overrideController.ShowInfo(vehicleItem.gameObject))
                {
                    return;
                }
            }

            SetLabel(vehicleItem.Label);
            SetDescription(vehicleItem.description);
        }
    }
}

