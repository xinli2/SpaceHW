using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Provides a physical location as well as a reference point for a module that adds functionality to a vehicle.
    /// </summary>
    public class ModuleMount : MonoBehaviour 
	{

        [Header("General")]
	
        // The label shown in the loadout menu for this module mount
		[SerializeField]
		protected string label = "Module Mount";
		public string Label { get { return label; } }

        [SerializeField]
        protected string m_ID = "Module Mount";
        public string ID { get { return m_ID; } }

        [SerializeField]
        protected bool specifyMountableTypes;

		// All the module types that can be mounted on this module mount
		[SerializeField]
		protected List<ModuleType> mountableTypes = new List<ModuleType>();
		public List<ModuleType> MountableTypes { get { return mountableTypes; } }

        [Tooltip("When checked, modules added to the module mount will be parented to the mount transform at the same position and rotation as the mount.")]
        [SerializeField]
        protected bool parentModulesToMount = true;
        
        /// <summary>
        /// A list of all the attachment points for mounting multi-unit modules
        /// </summary>
		[SerializeField]
        protected List<Transform> attachmentPoints = new List<Transform>();
        public List<Transform> AttachmentPoints { get { return attachmentPoints; } }

        // Whether to look for and mount the first available module at the start
        public bool mountFirstAvailableModuleAtStart = true;

        // Reference to the root object of the owner of this module (e.g. a vehicle)
        protected Transform rootTransform;
        public Transform RootTransform
        {
            set
            {
                rootTransform = value;

                // Update the root transform on existing modules
                for (int i = 0; i < modules.Count; ++i)
                {
                    modules[i].SetRootTransform(rootTransform);
                }
            }
        }

        [SerializeField]
        protected int sortingIndex = 0;
        public int SortingIndex { get { return sortingIndex; } }

        [Tooltip("Whether to set the layer of the module.")]
        [SerializeField]
        protected bool setModuleLayer;

        [Tooltip("The layer to set the module to.")]
        [SerializeField]
        protected string moduleLayer = "Default";

        [Header("Child Modules")]

        // Whether to load modules that already exist in the hierarchy
        public bool loadExistingChildModules = true;
        
        [Header("Default Modules")]

        // All the module prefabs that will be instantiated by default for this mount
        [SerializeField]
		protected List<Module> defaultModulePrefabs = new List<Module>();	
		public List<Module> DefaultModulePrefabs { get { return defaultModulePrefabs; } }

        // Whether or not to create (instantiate) default modules at the start
        public bool createDefaultModulesAtStart = true;
		
		// List of all the modules that have been created at this mount and which can be mounted here
		protected List<Module> modules = new List<Module>();	
		public List<Module> Modules { get { return modules; } }

		// The index of the currently selected module
		protected int mountedModuleIndex = -1;
		public int MountedModuleIndex { get { return mountedModuleIndex; } }
	
        [Header("Events")]

        public ModuleEventHandler onModuleMounted;

        public ModuleEventHandler onModuleUnmounted;

        [HideInInspector]
        public ModuleEventHandler onModuleAdded;

        [HideInInspector]
        public ModuleEventHandler onModuleRemoved;



        // Called when the component is first added to a gameobject or reset in the inspector
        protected virtual void Reset()
        {
            rootTransform = transform.root;
        }


        // Called when scene starts
        protected virtual void Start()
		{
            // Load all of the modules already existing as children of this module mount
            if (loadExistingChildModules)
            {
                Module[] existingModulesArray = transform.GetComponentsInChildren<Module>();
                foreach (Module existingModule in existingModulesArray)
                {
                    if (!IsCompatible(existingModule))
                    {
                        Debug.LogWarning("Skipping loading of child module as it is an incompatible type for this module mount.");
                        continue;
                    }
                  
                    // Check if this module is already loaded
                    bool found = false;
                    for (int i = 0; i < modules.Count; ++i)
                    {
                        if (modules[i] == existingModule) found = true;
                    }
                    if (found) continue;

                    // Center and orient the module at the module mount
                    existingModule.transform.localPosition = Vector3.zero;
                    existingModule.transform.localRotation = Quaternion.identity;

                    // Add the module as a mountable module
                    AddModule(existingModule, (mountFirstAvailableModuleAtStart && mountedModuleIndex == -1));
                    
                }
            }
            
			// Create all of the modules in the default list
			if (createDefaultModulesAtStart)
			{
				for (int i = 0; i < defaultModulePrefabs.Count; ++i)
				{
                    // If this listing is null, ignore it
                    if (defaultModulePrefabs[i] == null)
						continue;

                    // If this type of module is not allowed at the module mount, ignore it
					if (specifyMountableTypes && !mountableTypes.Contains(defaultModulePrefabs[i].ModuleType))
                    {
                        Debug.LogWarning("Skipping instantiation and loading of default module prefab as it is an incompatible type for this module mount.");
                        continue;
                    }
						
                    Module createdModule = Instantiate(defaultModulePrefabs[i], parentModulesToMount ? transform : null);
                    
					createdModule.transform.localPosition = Vector3.zero;
					createdModule.transform.localRotation = Quaternion.identity;
                    
					AddModule(createdModule, (mountFirstAvailableModuleAtStart && mountedModuleIndex == -1));
					
				}
			}
		}


        public virtual bool IsCompatible(Module module)
        {
            // If this type of module is not allowed here, ignore it
            if (specifyMountableTypes && !mountableTypes.Contains(module.ModuleType))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Add a new mountable module to this module mount.
        /// </summary>
        /// <param name="module">The module to be added (must be already created in the scene).</param>
        /// <param name="mountImmediately">Whether the module should be mounted immediately. </param>
        public void AddModule(Module module, bool mountImmediately = false)
		{

            if (specifyMountableTypes && !mountableTypes.Contains(module.ModuleType))
				return;

            // Update the module
            module.SetRootTransform(rootTransform);

            if (setModuleLayer)
            {
                int moduleLayerValue = LayerMask.NameToLayer(moduleLayer);
                if (moduleLayerValue != -1)
                {
                    module.SetLayer(moduleLayerValue);
                }
                else
                {
                    Debug.LogWarning("Failed to set module layer, layer name does not exist. Please add the layer first.");
                }
            }

            if (parentModulesToMount)
            {
                module.transform.SetParent(transform);
                module.transform.localPosition = Vector3.zero;
                module.transform.localRotation = Quaternion.identity;
            }
            
            module.gameObject.SetActive(false);

            // Add to mountable modules list
			modules.Add(module);

            // Call the event
            onModuleAdded.Invoke(module);
			
            // Mount
			if (mountImmediately)
			{ 
				MountModule(module);
			}
			else
			{
				module.Unmount();
			}
		}


        public void RemoveModule(Module moduleToRemove)
        {
            int index = modules.IndexOf(moduleToRemove);

            if (index == -1) return;

            // Unmount if mounted
            if (index == mountedModuleIndex)
            {
                UnmountActiveModule();
            }

            // Remove from list
            modules.RemoveAt(index);

            // Call the event
            onModuleRemoved.Invoke(moduleToRemove);

        }

        /// <summary>
        /// Remove all the modules that have been added to the module mount.
        /// </summary>
        public virtual void RemoveAllModules()
        {
            Module[] modulesArray = modules.ToArray();
            foreach(Module module in modulesArray)
            {
                RemoveModule(module);
            }
        }
		

		/// <summary>
        /// Cycle the mounted module at this module mount.
        /// </summary>
        /// <param name="forward">Whether to cycle forward or backward.</param>
		public virtual void Cycle(bool forward = true)
		{

			if (modules.Count <= 1) return;

			// Increment or decrement the module index
			int newMountedModuleIndex = forward ? mountedModuleIndex + 1 : mountedModuleIndex - 1;
		
			// Wrap forward - f exceeds highest index, return to zero index
			newMountedModuleIndex = newMountedModuleIndex >= modules.Count ? 0 : newMountedModuleIndex;

			// Wrap backward - if exceeds lowest index, return to last index
			newMountedModuleIndex = newMountedModuleIndex < 0 ? modules.Count - 1 : newMountedModuleIndex;

			// Mount the new Module
			MountModule(newMountedModuleIndex);

		}
		
		
		/// <summary>
        /// Mount a new module at the module mount. Module must be already added as a MountableModule instance.
        /// </summary>
        /// <param name="newMountedModuleIndex">The new module's index within the list of Mountable Modules.</param>
		public virtual void MountModule(int newMountedModuleIndex)
		{
            if (newMountedModuleIndex >= 0)
            {
                if (newMountedModuleIndex < modules.Count)
                {
                    MountModule(modules[newMountedModuleIndex]);
                }
            }
            else
            {
                MountModule((Module)null);
            }
        }


        /// <summary>
        /// Unmount the module currently mounted here.
        /// </summary>
        public virtual void UnmountActiveModule()
        {
            if (mountedModuleIndex >= 0)
            {
                modules[mountedModuleIndex].Unmount();
                modules[mountedModuleIndex].gameObject.SetActive(false);
                onModuleUnmounted.Invoke(modules[mountedModuleIndex]);
                mountedModuleIndex = -1;
            }
        }


        /// <summary>
        /// Mount a specified module at this module mount.
        /// </summary>
        /// <param name="module">The module to mount.</param>
        public virtual void MountModule(Module module)
        {
            UnmountActiveModule();

            if (module == null) return;

            // If it's not already added to the module mount, add it and mount immediately
            if (modules.IndexOf(module) == -1)
            {
                AddModule(module, true);
            }
            else
            {
                // Mount the module
                module.gameObject.SetActive(true);
                module.Mount(this);
                mountedModuleIndex = modules.IndexOf(module);
                onModuleMounted.Invoke(module);
            }
        }


        /// <summary>
        /// Mount a module at the module mount that has the specified ID.
        /// </summary>
        /// <param name="moduleID">The ID of the Module.</param>
        public virtual void MountModule(string moduleID)
        {
            for(int i = 0; i < modules.Count; ++i)
            {
                if (modules[i].ID == moduleID)
                {
                    MountModule(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Clear all of the modules stored at this module mount
        /// </summary>
        public virtual void UnmountAllModules()
		{
			for (int i = 0; i < modules.Count; ++i)
            {
                modules[i].Unmount();
                modules[i].gameObject.SetActive(false);
            }
            modules.Clear();
			mountedModuleIndex = -1;
		}


		/// <summary>
        /// Get a reference to the Module component of the module currently mounted at this module mount.
        /// </summary>
        /// <returns>The mounted module's Module component</returns>
		public virtual Module MountedModule()
		{
			if (mountedModuleIndex == -1)
			{
				return null;
			}
			else
			{
				return modules[mountedModuleIndex];
			}
		}
	}
}
