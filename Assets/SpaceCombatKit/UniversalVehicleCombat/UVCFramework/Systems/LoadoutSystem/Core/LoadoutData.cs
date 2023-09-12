using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Holds loadout data that can be saved or loaded.
    /// </summary>
    [System.Serializable]
    public class LoadoutData
    {
        [Tooltip("The currently selected slot (the one that the player chose in the loadout menu)")]
        public int selectedSlotIndex = -1;
        public virtual LoadoutSlot SelectedSlot { get { return selectedSlotIndex == -1 ? null : Slots[selectedSlotIndex]; } }

        [Tooltip("The loadout slots that are part of this")]
        public List<LoadoutSlot> Slots = new List<LoadoutSlot>();

        public LoadoutData()
        {
            selectedSlotIndex = -1;
        }
    }
}
