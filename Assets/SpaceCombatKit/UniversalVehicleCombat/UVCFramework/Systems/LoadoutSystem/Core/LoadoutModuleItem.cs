using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Loadout
{
    public class LoadoutModuleItem : MonoBehaviour
    {
        [Tooltip("The prefab for this loadout module.")]
        public Module modulePrefab;

        [Tooltip("Whether to override the Label variable on the Module component when displaying the label for this module.")]
        public bool overrideModuleLabel = false;

        [Tooltip("The label to display when the label is being overridden.")]
        public string overrideLabel = "Label Override";

        public virtual string Label
        {
            get 
            {
                if (overrideModuleLabel) 
                    return overrideLabel;
                else
                    return (modulePrefab != null && modulePrefab.Labels.Count > 0) ? modulePrefab.Labels[0] : ""; 
            }
        }

        [Tooltip("The description to display in the loadout menu for this module.")]
        [TextArea]
        public string description;

        [Tooltip("All the sprites associated with this module.")]
        public List<Sprite> sprites = new List<Sprite>();
    }

}
