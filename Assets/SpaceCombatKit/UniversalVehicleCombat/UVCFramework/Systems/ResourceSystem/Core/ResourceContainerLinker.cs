using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;

namespace VSX.ResourceSystem
{
    /// <summary>
    /// Link a 'Resource Container Interfacer' on a module to a 'Resource Container' on the vehicle.
    /// </summary>
    public class ResourceContainerLinker : ModuleManager
    {

        // All the resource containers on the vehicle.
        protected List<ResourceContainerBase> resourceContainers = new List<ResourceContainerBase>();

        [Tooltip("The resource types to link. Leave empty if all resource types are allowed to be linked.")]
        [SerializeField]
        protected List<ResourceType> specifiedResourceTypes = new List<ResourceType>();


        protected override void Awake()
        {
            resourceContainers = new List<ResourceContainerBase>(transform.GetComponentsInChildren<ResourceContainerBase>());

            // Ignore any resource containers found on modules
            Module[] modules = transform.GetComponentsInChildren<Module>();
            for (int i = 0; i < resourceContainers.Count; ++i)
            {
                bool found = false;
                foreach(Module module in modules)
                {
                    if (resourceContainers[i].transform.IsChildOf(module.transform))
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    resourceContainers.RemoveAt(i);
                    i--;
                }
            }

            base.Awake();
        }


        protected override void OnModuleMounted(Module module)
        {
            ResourceContainerInterfacer[] resourceContainerInterfacers = module.GetComponentsInChildren<ResourceContainerInterfacer>();
            foreach (ResourceContainerInterfacer interfacer in resourceContainerInterfacers)
            {
                foreach (ResourceContainerBase resourceContainer in resourceContainers)
                {
                    if ((specifiedResourceTypes.Count == 0 || specifiedResourceTypes.IndexOf(interfacer.ResourceType) != -1) && interfacer.ResourceType == resourceContainer.ResourceType)
                    {
                        interfacer.Container = resourceContainer;
                    }
                }
            }
        }


        protected override void OnModuleUnmounted(Module module)
        {
            ResourceContainerInterfacer[] resourceContainerInterfacers = module.GetComponentsInChildren<ResourceContainerInterfacer>();
            foreach (ResourceContainerInterfacer interfacer in resourceContainerInterfacers)
            {
                foreach (ResourceContainerBase resourceContainer in resourceContainers)
                {
                    if (interfacer.Container == resourceContainer)
                    {
                        interfacer.Container = null;
                    }
                }
            }
        }
    }
}
