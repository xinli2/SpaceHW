using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Manages the display of gun weapon info on the loadout
    /// </summary>
    public class GunWeaponInfoOverrideController : LoadoutItemInfoOverrideController
    {
        [Tooltip("The value to display for an infinite value, e.g. a beam weapon that has infinite speed.")]
        [SerializeField]
        protected string infiniteValueDisplay = "-";

        [Tooltip("The health types to show damage stats for.")]
        [SerializeField]
        protected List<HealthType> damageStatsHealthTypes = new List<HealthType>();


        /// <summary>
        /// Show the gun weapon info.
        /// </summary>
        /// <param name="item">The loadout item.</param>
        /// <returns>Whether the operation was successful (info displayed).</returns>
        public override bool ShowInfo (GameObject item)
        {
            LoadoutModuleItem loadoutModuleItem = item.GetComponent<LoadoutModuleItem>();
            if (loadoutModuleItem == null) return false;
            
            GunWeapon gunWeapon = loadoutModuleItem.modulePrefab.GetComponentInChildren<GunWeapon>();
            if (gunWeapon == null)
            {
                return false;
            }
            else
            {
                itemInfoController.SetLabel(loadoutModuleItem.Label);
                itemInfoController.SetDescription(loadoutModuleItem.description);
                if (loadoutModuleItem.sprites.Count > 0) itemInfoController.SetIcon(loadoutModuleItem.sprites[0]);

                // Show speed
                StatsInstance speedStatsInstance = itemInfoController.GetStatsInstance();
                string speedValueDisplay = gunWeapon.Speed == Mathf.Infinity ? infiniteValueDisplay : ((int)(gunWeapon.Speed)).ToString();
                speedStatsInstance.Set("SPEED", speedValueDisplay + " M/S");

                // Show range
                StatsInstance rangeStatsInstance = itemInfoController.GetStatsInstance();
                string rangeValueDisplay = gunWeapon.Range == Mathf.Infinity ? infiniteValueDisplay : ((int)(gunWeapon.Range)).ToString();
                rangeStatsInstance.Set("RANGE", rangeValueDisplay + " M");

                // Update damage stats
                for (int i = 0; i < damageStatsHealthTypes.Count; ++i)
                {
                    StatsInstance damageStatsInstance = itemInfoController.GetStatsInstance();
                    string damageStatsLabel = damageStatsHealthTypes[i].name.ToUpper() + " DMG";
                    string damageStatsValue = ((int)(gunWeapon.Damage(damageStatsHealthTypes[i]))).ToString() + " DPS";
                    damageStatsInstance.Set(damageStatsLabel, damageStatsValue);
                }

                return true;
            }
        }
    }
}

