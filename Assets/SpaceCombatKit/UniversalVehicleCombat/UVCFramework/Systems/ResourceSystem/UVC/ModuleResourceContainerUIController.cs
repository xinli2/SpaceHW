using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.ResourceSystem;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Display UI for a resource container found on a module mounted on a module mount.
    /// </summary>
    public class ModuleResourceContainerUIController : ResourceContainerUIController
    {

        [Tooltip("The module mount to display the resource container UI for.")]
        [SerializeField]
        protected ModuleMount moduleMount;


        protected override void Awake()
        {
            base.Awake();
            if (moduleMount != null)
            {
                moduleMount.onModuleMounted.AddListener(OnModuleMounted);
                moduleMount.onModuleUnmounted.AddListener(OnModuleUnmounted);

                if (moduleMount.MountedModule() != null) OnModuleMounted(moduleMount.MountedModule());
            }
        }


        protected virtual void OnModuleMounted(Module module)
        {
            ResourceContainer = module.GetComponent<ResourceContainer>();
        }


        protected virtual void OnModuleUnmounted(Module module)
        {
            ResourceContainer = null;
        }
    }
}
