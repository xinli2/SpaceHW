using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Display info for a loadout module.
    /// </summary>
    public class LoadoutModuleInfoController : LoadoutItemInfoController
    {

        /// <summary>
        /// Update the loadout module info.
        /// </summary>
        public override void UpdateInfo()
        {

            base.UpdateInfo();

            if (loadoutManager.Items == null) { Hide(); return; }
            if (loadoutManager.SelectedModuleMountIndex == -1) { Hide(); return; }
            if (loadoutManager.WorkingSlot.selectedVehicleIndex == -1) { Hide(); return; }
            if (loadoutManager.WorkingSlot.selectedModules[loadoutManager.SelectedModuleMountIndex] == -1) { Hide(); return; }
            
            LoadoutModuleItem moduleItem = loadoutManager.Items.modules[loadoutManager.WorkingSlot.selectedModules[loadoutManager.SelectedModuleMountIndex]];

            foreach (LoadoutItemInfoOverrideController overrideController in overrideControllers)
            {
                if (overrideController.ShowInfo(moduleItem.gameObject))
                {
                    return;
                }
            }
            Debug.Log("AHA");
            SetLabel(moduleItem.Label);
            SetDescription(moduleItem.description);
            if (moduleItem.sprites.Count > 0) SetIcon(moduleItem.sprites[0]);
        }
    }
}

