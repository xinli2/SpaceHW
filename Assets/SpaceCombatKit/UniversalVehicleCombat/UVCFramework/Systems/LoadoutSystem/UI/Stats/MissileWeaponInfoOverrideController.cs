using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Display loadout info for a missile.
    /// </summary>
    public class MissileWeaponInfoOverrideController : LoadoutItemInfoOverrideController
    {

        [Tooltip("The value to display for an infinite value.")]
        [SerializeField]
        protected string infiniteValueDisplay = "-";

        [Tooltip("The health types to show damage stats for.")]
        [SerializeField]
        protected List<HealthType> damageStatsHealthTypes = new List<HealthType>();


        /// <summary>
        /// Show the missile weapon info.
        /// </summary>
        /// <param name="item">The missile loadout item.</param>
        /// <returns>Whether the info was successfully displayed.</returns>
        public override bool ShowInfo(GameObject item)
        {

            LoadoutModuleItem loadoutModuleItem = item.GetComponent<LoadoutModuleItem>();
            if (loadoutModuleItem == null) return false;

            Module module = loadoutModuleItem.modulePrefab.GetComponentInChildren<Module>();
            if (module == null) return false;

            MissileWeapon missileWeapon = loadoutModuleItem.modulePrefab.GetComponentInChildren<MissileWeapon>();

            if (missileWeapon == null)
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
                string speedValueDisplay = missileWeapon.Speed == Mathf.Infinity ? infiniteValueDisplay : ((int)(missileWeapon.Speed)).ToString();
                speedStatsInstance.Set("SPEED", speedValueDisplay + " M/S");

                // Show range
                StatsInstance rangeStatsInstance = itemInfoController.GetStatsInstance();
                string rangeValueDisplay = missileWeapon.Range == Mathf.Infinity ? infiniteValueDisplay : ((int)(missileWeapon.Range)).ToString();
                rangeStatsInstance.Set("RANGE", rangeValueDisplay + " M");

                // Update damage stats
                for (int i = 0; i < damageStatsHealthTypes.Count; ++i)
                {
                    StatsInstance damageStatsInstance = itemInfoController.GetStatsInstance();
                    string damageStatsLabel = damageStatsHealthTypes[i].name.ToUpper() + " DMG";
                    string damageStatsValue = ((int)(missileWeapon.Damage(damageStatsHealthTypes[i]))).ToString() + " DPS";
                    damageStatsInstance.Set(damageStatsLabel, damageStatsValue);
                }

                return true;
            }
        }
    }
}

