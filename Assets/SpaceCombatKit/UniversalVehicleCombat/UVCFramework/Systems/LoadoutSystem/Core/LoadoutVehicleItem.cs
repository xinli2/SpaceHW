using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat.Loadout
{
    public class LoadoutVehicleItem : MonoBehaviour
    {
        [Tooltip("The vehicle prefab associated with this loadout item.")]
        public Vehicle vehiclePrefab;

        [Tooltip("Whether to override the label of the Vehicle component when displaying this vehicle in the loadout menu.")]
        public bool overrideVehicleLabel = false;

        [Tooltip("The label value to display in the loadout menu when overriding the label on the Vehicle component.")]
        public string overrideLabel = "Label Override";

        public virtual string Label
        {
            get { return overrideVehicleLabel ? overrideLabel : vehiclePrefab.Label; }
        }

        [Tooltip("The description to display in the loadout menu for this vehicle.")]
        [TextArea]
        public string description;

        [Tooltip("All the sprites associated with this vehicle (can be looked up by index).")]
        public List<Sprite> sprites = new List<Sprite>();

        [Tooltip("The default module loadout for the vehicle (displayed when no saved data is found).")]
        public List<Module> defaultLoadout = new List<Module>();

        [Tooltip("Whether this vehicle is currently locked and unavailable, or is unlocked and available in the loadout.")]
        public bool locked = false;

        [Tooltip("The offset for the camera from the module mount position when focusing on a module on this vehicle.")]
        public Vector3 moduleMountViewAlignmentOffset = Vector3.zero;
    }

}
