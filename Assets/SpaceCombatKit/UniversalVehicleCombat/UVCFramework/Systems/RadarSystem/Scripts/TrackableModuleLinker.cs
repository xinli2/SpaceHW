using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    public class TrackableModuleLinker : ModuleManager
    {
        [SerializeField]
        protected Trackable parentTrackable;

        protected override void OnModuleMounted(Module module)
        {
            base.OnModuleMounted(module);

            Trackable moduleTrackable = module.GetComponent<Trackable>();
            if (parentTrackable != null && moduleTrackable != null)
            {
                parentTrackable.AddChildTrackable(moduleTrackable);
            }
        }
    }
}

