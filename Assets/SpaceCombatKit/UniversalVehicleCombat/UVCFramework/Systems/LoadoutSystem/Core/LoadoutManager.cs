using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// This class manages the loadout menu scene.
    /// </summary>
    public class LoadoutManager : MonoBehaviour
    {

        [Tooltip("The loadout items (vehicles and modules) to add when the scene starts.")]
        [SerializeField]
        protected LoadoutItems startingItems;
        protected LoadoutItems items;
        public virtual LoadoutItems Items
        {
            get { return items; }
        }

        [Tooltip("Whether a vehicle item is exclusive when assigned to a slot, i.e. it cannot be selected at another slot.")]
        [SerializeField]
        protected bool exclusiveVehicles = false;


        [Tooltip("Whether a module item is exclusive when assigned to a vehicle, i.e. it cannot be selected on another vehicle.")]
        [SerializeField]
        protected bool exclusiveModules = false;   // Whether modules are exclusive when assigned to vehicles


        [Tooltip("Whether to create one slot per vehicle item. This enables a loadout where different vehicle loadouts can be set up and saved but only one chosen. Will override 'Num Slots'.")]
        [SerializeField]
        protected bool slotPerVehicle = true;
        public bool SlotPerVehicle { get { return slotPerVehicle; } }

        [Tooltip("How many loadout slots to create.")]
        [SerializeField]
        protected int numSlots = 1;

        protected LoadoutSlot workingSlot;  // A slot that represents the current state of the loadout in terms of visualization, but data may not be finalized
        public virtual LoadoutSlot WorkingSlot { get { return workingSlot; } }

        [Tooltip("Whether to apply the vehicle selection as soon as it changes, i.e. if the player cycles through vehicles, they won't have to press any 'Select Vehicle' button to finalize their choice.")]
        [SerializeField]
        protected bool applyVehicleSelection = true;


        [Tooltip("Whether to apply the module selection as soon as it changes, i.e. if the player cycles through modules, they won't have to press any 'Select Module' button to finalize their choice.")]
        [SerializeField]
        protected bool applyModuleSelection = false;


        // Selection parameters

        protected int selectedModuleMountIndex = -1;
        public virtual int SelectedModuleMountIndex { get { return selectedModuleMountIndex; } }


        protected List<int> selectableVehicleIndexes = new List<int>();
        public virtual List<int> SelectableVehicleIndexes { get { return selectableVehicleIndexes; } }  // Indexes refer to the current LoadoutItems vehicle and module lists


        protected List<int> selectableModuleIndexes = new List<int>();
        public virtual List<int> SelectableModuleIndexes { get { return selectableModuleIndexes; } }    // Indexes refer to the current LoadoutItems vehicle and module lists

        [Tooltip("The component that handles loadout data saving. Can be changed at runtime.")]
        [SerializeField]
        protected LoadoutDataManager loadoutDataManager;
        public LoadoutDataManager LoadoutDataManager
        {
            get { return loadoutDataManager; }
            set { loadoutDataManager = value; }
        }

        [Tooltip("Whether to load the last selected slot or just start at the first slot after loading.")]
        [SerializeField]
        protected bool loadSelectedSlot = true;

        [Tooltip("Called when the loadout data is created or loaded.")]
        public UnityEvent onDataLoad;

        [Tooltip("Called when something on the loadout changes, so that UI and other components can update.")]
        public UnityEvent onLoadoutChanged;

        protected LoadoutData loadoutData;
        public LoadoutData LoadoutData { get { return loadoutData; } }




        protected virtual void Reset()
        {
            loadoutDataManager = FindObjectOfType<LoadoutDataManager>();
        }


        protected virtual void Awake()
        {
            loadoutData = new LoadoutData();

            InitializeWorkingSlot();
        }


        protected virtual void Start()
        {
            if (items == null) SetItems(startingItems);
        }


        // Initialize the working slot
        protected virtual void InitializeWorkingSlot()
        {
            workingSlot = new LoadoutSlot();
            workingSlot.selectedVehicleIndex = -1;
        }


        // Called when the loadout items change
        protected virtual void OnDataLoad()
        {
            onDataLoad.Invoke();
        }


        // Called when something on the loadout changes, e.g. a vehicle or module selection occurs
        protected virtual void OnLoadoutChanged()
        {
            onLoadoutChanged.Invoke();
        }


        /// <summary>
        /// Set the loadout items.
        /// </summary>
        /// <param name="items">The new items.</param>
        public virtual void SetItems(LoadoutItems items, bool loadPersistentData = true)
        {

            this.items = items;

            if (loadPersistentData)
            {
                LoadPersistentData();
            }

            if (loadoutData.Slots.Count == 0)
            {
                LoadDefaultData();
            }
        }


        // Slots


        // Initialize/build the slots from the loadout data
        protected virtual void LoadDefaultData()
        {

            // Create the slots
            loadoutData.Slots = new List<LoadoutSlot>();

            numSlots = (slotPerVehicle && items != null) ? items.vehicles.Count : Mathf.Max(numSlots, 1);
            for (int i = 0; i < numSlots; ++i)
            {
                LoadoutSlot newSlot = new LoadoutSlot();
                newSlot.selectedVehicleIndex = -1;
                loadoutData.Slots.Add(newSlot);
            }

            if (items == null) return;


            // Fill the slots with data

            if (items != null)
            {
                for (int i = 0; i < loadoutData.Slots.Count; ++i)
                {
                    loadoutData.Slots[i].selectedVehicleIndex = i < items.vehicles.Count ? i : -1;

                    if (loadoutData.Slots[i].selectedVehicleIndex != -1)
                    {
                        List<int> defaultModules = GetDefaultModules(loadoutData.Slots[i].selectedVehicleIndex);

                        foreach (int defaultModule in defaultModules)
                        {
                            bool used = false;
                            if (exclusiveModules)
                            {
                                foreach (LoadoutSlot slot in loadoutData.Slots)
                                {
                                    foreach (int usedModuleIndex in slot.selectedModules)
                                    {
                                        if (usedModuleIndex == defaultModule)
                                        {
                                            used = true;
                                            break;
                                        }
                                    }
                                    if (used) break;
                                }
                            }

                            if (!used)
                            {
                                loadoutData.Slots[i].selectedModules.Add(defaultModule);
                            }
                            else
                            {
                                loadoutData.Slots[i].selectedModules.Add(-1);
                            }
                        }
                    }
                }
            }

            OnDataLoad();

            loadoutData.selectedSlotIndex = -1;
            if (loadoutData.Slots.Count > 0) SelectSlot(0);

            OnLoadoutChanged();
        }


        /// <summary>
        /// Select a loadout slot.
        /// </summary>
        /// <param name="slotIndex">The index of the slot.</param>
        public virtual void SelectSlot(int slotIndex)
        {
            slotIndex = Mathf.Clamp(slotIndex, -1, loadoutData.Slots.Count - 1);

            loadoutData.selectedSlotIndex = slotIndex;

            RevertWorkingToActiveSlot();

            UpdateSelectableVehicles();

            SelectModuleMount(0);

            OnLoadoutChanged();

        }


        /// <summary>
        /// Cycle through the loadout slots forward or back.
        /// </summary>
        /// <param name="forward">Whether to cycle forward (back if false)</param>
        /// <param name="wrap">Whether to wrap around to beginning when cycling past the end, or wrap to the end when cycling back past the beginning.</param>
        public virtual void CycleSlot(bool forward, bool wrap = false)
        {
            // Cycle forward or back

            int index = loadoutData.selectedSlotIndex;
            if (forward)
            {
                index++;
            }
            else
            {
                index--;
            }

            // Wrap

            if (wrap)
            {
                if (index < 0)
                {
                    index = loadoutData.Slots.Count - 1;
                }
                else if (index >= loadoutData.Slots.Count)
                {
                    index = Mathf.Min(0, loadoutData.Slots.Count - 1);
                }
            }
            else
            {
                index = Mathf.Clamp(index, 0, loadoutData.Slots.Count - 1);
            }

            // Select the new slot
            SelectSlot(index);
        }



        /// <summary>
        /// Clear the selected slot.
        /// </summary>
        public virtual void ClearSelectedSlot()
        {
            if (loadoutData.SelectedSlot != null && loadoutData.SelectedSlot.selectedVehicleIndex != -1)
            {
                loadoutData.SelectedSlot.selectedVehicleIndex = -1;
                loadoutData.SelectedSlot.selectedModules.Clear();
                RevertWorkingToActiveSlot();

                OnLoadoutChanged();
            }
        }


        // Get the default module loadout for a vehicle
        protected virtual List<int> GetDefaultModules(int vehicleIndex)
        {
            List<int> defaultModuleIndexes = new List<int>();

            LoadoutVehicleItem vehicleItem = items.vehicles[vehicleIndex];

            int numModuleMounts = items.vehicles[vehicleIndex].vehiclePrefab.GetComponentsInChildren<ModuleMount>().Length;

            for(int i = 0; i < numModuleMounts; ++i)
            {
                if (i >= items.vehicles[vehicleIndex].defaultLoadout.Count)
                {
                    defaultModuleIndexes.Add(-1);
                }
                else
                {
                    int index = -1;
                    for (int j = 0; j < items.modules.Count; ++j)
                    {
                        if (items.modules[j].modulePrefab == vehicleItem.defaultLoadout[i])
                        {
                            index = j;
                            break;
                        }
                    }

                    defaultModuleIndexes.Add(index);
                }
            }

            return defaultModuleIndexes;
        }


        // Vehicle selection


        /// <summary>
        /// Select a vehicle in the loadout menu.
        /// </summary>
        /// <param name="index">The index of the vehicle to select.</param>
        public virtual void SelectVehicle(int vehicleIndex)
        {
            if (selectableVehicleIndexes.IndexOf(vehicleIndex) == -1) return;
            if (vehicleIndex == workingSlot.selectedVehicleIndex) return;
           
            workingSlot.selectedVehicleIndex = vehicleIndex;

            // Update the working slot

            if (vehicleIndex == loadoutData.SelectedSlot.selectedVehicleIndex)
            {

                RevertWorkingToActiveSlot();
            }
            else
            {
                // Initialize the module selection

                List<int> defaultModules = GetDefaultModules(workingSlot.selectedVehicleIndex);

                workingSlot.selectedModules.Clear();

                foreach (int defaultModule in defaultModules)
                {
                    bool used = false;
                    if (exclusiveModules)
                    {
                        foreach (LoadoutSlot slot in loadoutData.Slots)
                        {
                            foreach (int usedModuleIndex in slot.selectedModules)
                            {
                                if (usedModuleIndex == defaultModule)
                                {
                                    used = true;
                                    break;
                                }
                            }
                            if (used) break;
                        }
                    }

                    if (!used)
                    {
                        workingSlot.selectedModules.Add(defaultModule);
                    }
                    else
                    {
                        workingSlot.selectedModules.Add(-1);
                    }
                }
            }

            if (applyVehicleSelection) SaveWorkingToActiveSlot();

            SelectModuleMount(0);
            
            OnLoadoutChanged();

        }


        /// <summary>
        /// Cycle through the vehicles forward or back.
        /// </summary>
        /// <param name="forward">Whether to cycle forward (back if false)</param>
        /// <param name="wrap">Whether to wrap around to beginning when cycling past the end, or wrap to the end when cycling back past the beginning.</param>
        public virtual void CycleVehicleSelection(bool forward, bool wrap = false)
        {

            // Cycle up or down
            int index = selectableVehicleIndexes.IndexOf(workingSlot.selectedVehicleIndex);

            if (forward)
            {
                index++;
            }
            else
            {
                index--;
            }

            // Wrap

            if (wrap)
            {
                if (index < 0)
                {
                    index = selectableVehicleIndexes.Count - 1;
                }
                else if (index >= selectableVehicleIndexes.Count)
                {
                    index = Mathf.Min(0, selectableVehicleIndexes.Count - 1);
                }
            }
            else
            {
                index = Mathf.Clamp(index, 0, selectableVehicleIndexes.Count - 1);
            }

            // Select the new vehicle
            if (index != -1) SelectVehicle(selectableVehicleIndexes[index]);

        }


        public virtual LoadoutVehicleItem GetSelectedVehicleItem()
        {
            if (loadoutData == null) return null;
            if (loadoutData.SelectedSlot == null) return null;
            if (loadoutData.SelectedSlot.selectedVehicleIndex == -1) return null;

            return items.vehicles[loadoutData.Slots[loadoutData.selectedSlotIndex].selectedVehicleIndex];
        }


        // Update the list of vehicles that can be selected at the current slot
        protected virtual void UpdateSelectableVehicles()
        {

            selectableVehicleIndexes.Clear();

            if (items == null) return;

            for (int i = 0; i < items.vehicles.Count; ++i)
            {
                bool used = false;

                if (exclusiveVehicles)
                {
                    foreach (LoadoutSlot slot in loadoutData.Slots)
                    {
                        if (loadoutData.Slots.IndexOf(slot) != loadoutData.selectedSlotIndex && slot.selectedVehicleIndex == i)
                        {
                            used = true;
                            break;
                        }
                    }
                }

                if (!used) selectableVehicleIndexes.Add(i);
            }
        }


        // Module mount selection


        /// <summary>
        /// Select a module mount on the selected vehicle.
        /// </summary>
        /// <param name="moduleMountIndex">The module mount index in the vehicle's Module Mounts list.</param>
        public virtual void SelectModuleMount(int moduleMountIndex)
        {
            selectedModuleMountIndex = Mathf.Min(moduleMountIndex, workingSlot.selectedModules.Count - 1);

            UpdateSelectableModules();

            RevertWorkingToActiveSlot();

            OnLoadoutChanged();

            
        }


        /// <summary>
        /// Cycle forward or back through the module mounts on the selected vehicle.
        /// </summary>
        /// <param name="forward">Whether to cycle forward (back if false)</param>
        /// <param name="wrap">Whether to wrap around to beginning when cycling past the end, or wrap to the end when cycling back past the beginning.</param>
        public virtual void CycleModuleMount(bool forward, bool wrap = false)
        {

            // Cycle forward or back

            int index = selectedModuleMountIndex;
            if (forward)
            {
                index++;
            }
            else
            {
                index--;
            }

            // Wrap

            if (wrap)
            {
                if (index < 0)
                {
                    index = items.vehicles[workingSlot.selectedVehicleIndex].vehiclePrefab.ModuleMounts.Count - 1;
                }
                else if (index >= items.vehicles[workingSlot.selectedVehicleIndex].vehiclePrefab.ModuleMounts.Count)
                {
                    index = Mathf.Min(0, items.vehicles[workingSlot.selectedVehicleIndex].vehiclePrefab.ModuleMounts.Count - 1);
                }
            }
            else
            {
                index = Mathf.Clamp(index, -1, items.vehicles[workingSlot.selectedVehicleIndex].vehiclePrefab.ModuleMounts.Count - 1);
            }

            // Select the new module mount

            SelectModuleMount(index);
        }


        /// <summary>
        /// Remove modules from the currently selected module mount.
        /// </summary>
        public virtual void ClearSelectedModuleMount()
        {
            SelectModule(-1);
        }



        public virtual ModuleMount GetSelectedModuleMount()
        {
            LoadoutVehicleItem vehicleItem = GetSelectedVehicleItem();
            if (vehicleItem == null) return null;

            if (selectedModuleMountIndex == -1) return null;

            return vehicleItem.vehiclePrefab.ModuleMounts[selectedModuleMountIndex];

        }

        // Update the list of modules that can be selected at the selected module mount
        protected virtual void UpdateSelectableModules()
        {

            selectableModuleIndexes.Clear();

            if (items == null) return;

            ModuleMount selectedModuleMount = GetSelectedModuleMount();
            if (selectedModuleMount == null) return;
            

            for (int i = 0; i < items.modules.Count; ++i)
            {
                if (!selectedModuleMount.IsCompatible(items.modules[i].modulePrefab)) continue;

                bool used = false;
                if (exclusiveModules)
                {
                    foreach (LoadoutSlot slot in loadoutData.Slots)
                    {
                        foreach (int usedModuleIndex in slot.selectedModules)
                        {
                            if (slot == loadoutData.SelectedSlot && slot.selectedModules.IndexOf(usedModuleIndex) == selectedModuleMountIndex) continue;


                            if (usedModuleIndex == i)
                            {
                                used = true;
                                break;
                            }
                        }
                        if (used) break;

                    }
                }


                if (!used) selectableModuleIndexes.Add(i);

            }
        }


        // Module selection


        /// <summary>
        /// Select a module item.
        /// </summary>
        /// <param name="newModuleIndex">The index of the module to select (in the loadout items Modules list).</param>
        public virtual void SelectModule(int newModuleIndex)
        {
            workingSlot.selectedModules[selectedModuleMountIndex] = newModuleIndex;
            if (applyModuleSelection) SaveWorkingToActiveSlot();
            OnLoadoutChanged();
        }


        /// <summary>
        /// Cycle through the modules forward or back.
        /// </summary>
        /// <param name="forward">Whether to cycle forward (back if false)</param>
        /// <param name="wrap">Whether to wrap around to beginning when cycling past the end, or wrap to the end when cycling back past the beginning.</param>
        public virtual void CycleModule(bool forward, bool wrap = false)
        {

            // Cycle forward or back

            int index = selectableModuleIndexes.IndexOf(workingSlot.selectedModules[selectedModuleMountIndex]);
            if (forward)
            {
                index++;
            }
            else
            {
                index--;
            }

            // Wrap

            if (wrap)
            {
                if (index < 0)
                {
                    index = selectableModuleIndexes.Count - 1;
                }
                else if (index >= selectableModuleIndexes.Count)
                {
                    index = Mathf.Min(0, selectableModuleIndexes.Count - 1);
                }
            }
            else
            {
                index = Mathf.Clamp(index, -1, selectableModuleIndexes.Count - 1);
            }

            // Select the new module

            SelectModule(selectableModuleIndexes[index]);
        }



        // Data saving


        /// <summary>
        /// Save the working slot to the currently selected slot.
        /// </summary>
        public virtual void SaveWorkingToActiveSlot()
        {
            if (loadoutData.SelectedSlot == null) return;

            loadoutData.SelectedSlot.selectedVehicleIndex = workingSlot.selectedVehicleIndex;
            loadoutData.SelectedSlot.selectedModules = new List<int>(workingSlot.selectedModules);

            OnLoadoutChanged();
        }


        /// <summary>
        /// Revert the working slot to the currently selected slot.
        /// </summary>
        public virtual void RevertWorkingToActiveSlot()
        {
            if (loadoutData.SelectedSlot == null)
            {
                workingSlot.selectedVehicleIndex = -1;
                workingSlot.selectedModules = new List<int>();
                return;
            }

            workingSlot.selectedVehicleIndex = loadoutData.SelectedSlot.selectedVehicleIndex;
            workingSlot.selectedModules = new List<int>(loadoutData.SelectedSlot.selectedModules);

            OnLoadoutChanged();
        }


        /// <summary>
        /// Save the loadout data persistently.
        /// </summary>
        public virtual void SavePersistentData()
        {
            if (loadoutData != null)
            {
                loadoutDataManager.SaveData(loadoutData);
            }
            else
            {
                Debug.LogWarning("Failed to save data. Please set the Loadout Data component in the inspector.");
            }
        }

        /// <summary>
        /// Load the saved loadout.
        /// </summary>
        public virtual void LoadPersistentData()
        {
            if (items == null) return;

            LoadoutData loadedData = loadoutDataManager.LoadData();

            if (loadedData != null)
            {
                if (loadedData.Slots.Count > 0)
                {
                    // Verify the vehicles

                    for (int i = 0; i < loadedData.Slots.Count; ++i)
                    {
                        if (loadedData.Slots[i].selectedVehicleIndex != -1)
                        {
                            // If the vehicle index is outside the range of the vehicles list, clear it
                            if (loadedData.Slots[i].selectedVehicleIndex >= items.vehicles.Count)
                            {
                                loadedData.Slots[i].selectedVehicleIndex = -1;
                            }
                            else
                            {
                                // Check exclusivity
                                if (exclusiveVehicles)
                                {
                                    for (int j = 0; j < i; ++j)
                                    {
                                        if (loadedData.Slots[i].selectedVehicleIndex == loadedData.Slots[j].selectedVehicleIndex)
                                        {
                                            loadedData.Slots[i].selectedVehicleIndex = -1;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Verify the number of module mounts

                    foreach (LoadoutSlot slot in loadedData.Slots)
                    {
                        if (slot.selectedVehicleIndex != -1)
                        {
                            int numModuleMountsOnVehicle = items.vehicles[slot.selectedVehicleIndex].vehiclePrefab.ModuleMounts.Count;
                            if (slot.selectedModules.Count > numModuleMountsOnVehicle)
                            {
                                slot.selectedModules.RemoveRange(numModuleMountsOnVehicle, slot.selectedModules.Count - numModuleMountsOnVehicle);
                            }
                            else if (slot.selectedModules.Count < numModuleMountsOnVehicle)
                            {
                                int numToAdd = numModuleMountsOnVehicle - slot.selectedModules.Count;
                                for (int i = 0; i < numToAdd; ++i)
                                {
                                    slot.selectedModules.Add(-1);
                                }
                            }
                        }
                        else
                        {
                            slot.selectedModules.Clear();
                        }
                    }

                    // Verify the modules

                    for (int i = 0; i < loadedData.Slots.Count; ++i)
                    {
                        if (loadedData.Slots[i].selectedVehicleIndex == -1) continue;

                        for (int j = 0; j < loadedData.Slots[i].selectedModules.Count; ++j)
                        {
                            if (loadedData.Slots[i].selectedModules[j] == -1) continue;

                            if (loadedData.Slots[i].selectedModules[j] >= items.modules.Count)
                            {
                                loadedData.Slots[i].selectedModules[j] = -1;
                                continue;
                            }

                            if (exclusiveModules)
                            {
                                for (int k = 0; k <= i; ++k)
                                {
                                    for (int l = 0; l < loadedData.Slots[k].selectedModules.Count; ++l)
                                    {
                                        if (k == i && l == j) continue;

                                        if (loadedData.Slots[k].selectedModules[l] == loadedData.Slots[i].selectedModules[j])
                                        {
                                            loadedData.Slots[i].selectedModules[j] = -1;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Verify the selected slot
                    if (loadedData.Slots.Count == 0)
                    {
                        loadedData.selectedSlotIndex = -1;
                    }
                    else
                    {
                        if (loadSelectedSlot)
                        {
                            if (loadedData.selectedSlotIndex < 0 || loadedData.selectedSlotIndex >= loadedData.Slots.Count)
                            {
                                loadedData.selectedSlotIndex = 0;
                            }
                        }
                        else
                        {
                            loadedData.selectedSlotIndex = 0;
                        }
                    }
                }
            }
            else
            {
                loadedData = new LoadoutData();
            }

            loadoutData = loadedData;

            OnDataLoad();

            SelectSlot(loadoutData.selectedSlotIndex);

            OnLoadoutChanged();
        }

        /// <summary>
        /// Delete the persistent data.
        /// </summary>
        /// <param name="loadDefaultData">Whether to load default loadout data after deleting saved loadout data.</param>
        public virtual void DeletePersistentData(bool loadDefaultData = true)
        {
            loadoutDataManager.DeleteSaveData();
            if (loadDefaultData)
            {
                LoadDefaultData();
            }
        }
    }
}