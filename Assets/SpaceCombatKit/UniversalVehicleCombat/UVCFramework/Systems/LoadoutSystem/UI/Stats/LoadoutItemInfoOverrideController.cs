using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// A base class for an override controller to display info about a loadout item.
    /// </summary>
    public class LoadoutItemInfoOverrideController : MonoBehaviour
    {

        [Tooltip("The base info controller.")]
        [SerializeField] protected LoadoutItemInfoController itemInfoController;
        public virtual LoadoutItemInfoController ItemInfoController
        {
            get { return itemInfoController; }
            set { itemInfoController = value; }
        }


        /// <summary>
        /// Show the info for a loadout item.
        /// </summary>
        /// <param name="item">The loadout item gameobject.</param>
        /// <returns>Whether info was successfully displayed.</returns>
        public virtual bool ShowInfo(GameObject item)
        {
            return false;
        }
    }
}
