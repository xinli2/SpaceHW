using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VSX.UniversalVehicleCombat;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Manages the UI for a loadout menu.
    /// </summary>
    public class LoadoutUIController : MonoBehaviour
    {
        [Tooltip("The loadout manager to display UI for.")]
        [SerializeField]
        protected LoadoutManager loadoutManager;


        [Header("Vehicle Selection")]


        [Tooltip("The gameobject to toggle to enable/disable the vehicle selection mode UI.")]
        [SerializeField]
        protected GameObject vehicleSelectionModeUIHandle;

        [Tooltip("The gameobject to toggle to enable/disable the vehicle info UI.")]
        [SerializeField]
        protected GameObject vehicleInfoUIHandle;

        [Tooltip("The button to select the previous vehicle in the loadout.")]
        [SerializeField]
        protected GameObject selectPreviousVehicleButton;

        [Tooltip("The button to select the next vehicle in the loadout.")]
        [SerializeField]
        protected GameObject selectNextVehicleButton;

        [Tooltip("Whether to wrap through the vehicle selection (go back to beginning when cycle past the end, go to the end when cycling back past beginning).")]
        [SerializeField]
        protected bool wrapVehicles = false;

        [Tooltip("The button to equip the highlighted vehicle (set it as the selected vehicle).")]
        [SerializeField]
        protected GameObject equipVehicleButton;

        [Tooltip("The button to go to the module loadout for the highlighted vehicle.")]
        [SerializeField]
        protected GameObject goToLoadoutButton;


        [Header("Module Selection")]

        [Tooltip("The gameobject to toggle to enable/disable the module selection UI.")]
        [SerializeField]
        protected GameObject moduleSelectionModeUIHandle;

        [Tooltip("The gameobject to toggle to enable/disable the module options list.")]
        [SerializeField]
        protected GameObject moduleOptionsUIHandle;

        [Tooltip("The controller for the module buttons list.")]
        [SerializeField]
        protected ButtonsListController moduleButtonsListController;

        [Tooltip("The gameobject to toggle to enable/disable the module info UI.")]
        [SerializeField]
        protected GameObject moduleInfoUIHandle;

        [Tooltip("The button to equip (select) the highlighted module.")]
        [SerializeField]
        protected GameObject equipModuleButton;


        [Header("Module Mount Selection")]

        [Tooltip("The controller for the module mounts buttons list.")]
        [SerializeField]
        protected ButtonsListController moduleMountButtonsListController;

        [Tooltip("The gameobject to toggle to enable/disable the module mounts list UI.")]
        [SerializeField]
        protected GameObject moduleMountOptionsUIHandle;

        [Tooltip("The gameobject to toggle to enable/disable the module mount info.")]
        [SerializeField]
        protected GameObject moduleMountInfoUIHandle;


        [Header("Slot Selection")]

        [Tooltip("Whether to show the loadout slots UI.")]
        [SerializeField]
        protected bool showSlots = true;

        [Tooltip("The slots buttons list controller.")]
        [SerializeField]
        protected ButtonsListController slotButtonsListController;

        [Tooltip("The gameobject to activate/deactivate to toggle the slots menu on/off.")]
        [SerializeField]
        protected Transform slotSelectionUIHandle;

        [Tooltip("The minimum number of slots that may be shown in the menu.")]
        [SerializeField]
        protected int minVisibleSlots = 2;

        [Tooltip("The sprite to set for the slot when it is empty.")]
        [SerializeField]
        protected Sprite slotEmptyIcon;

        [Tooltip("The vehicle sprite index to set for the slot (index refers to the index in the Sprites list in the Loadout Items vehicle reference for that vehicle).")]
        [SerializeField]
        protected int slotIconSpriteIndex = 0;


        [Header("Scene Loading")]

        [Tooltip("The main menu scene name in the Build Settings.")]
        [SerializeField]
        protected string mainMenuSceneName = "MCK_MainMenu";

        [Tooltip("The list of missions that can be selected in the loadout menu.")]
        [SerializeField]
        protected List<string> missionSceneNames = new List<string>();

        [Tooltip("Event called when the loadout menu goes into the vehicle selection mode.")]
        public UnityEvent onVehicleSelectionMode;

        [Tooltip("Event called when the loadout menu goes into the module selection mode.")]
        public UnityEvent onModuleSelectionMode;

        public enum UIState
        {
            VehicleSelection,
            ModuleSelection
        }

        protected UIState state;
        public virtual UIState State
        {
            get { return state; }
        }


        protected virtual void Awake()
        {
            loadoutManager.onLoadoutChanged.AddListener(OnLoadoutChanged);

            slotButtonsListController.onButtonClicked.AddListener(OnSlotClicked);
            moduleButtonsListController.onButtonClicked.AddListener(OnModuleClicked);
            moduleMountButtonsListController.onButtonClicked.AddListener(OnModuleMountClicked);
        }


        protected virtual void Start()
        {
            EnterVehicleSelection();
            OnLoadoutChanged();
        }


        /// <summary>
        /// Enter vehicle selection mode.
        /// </summary>
        public virtual void EnterVehicleSelection()
        {
            loadoutManager.RevertWorkingToActiveSlot();

            vehicleSelectionModeUIHandle.SetActive(true);
            moduleSelectionModeUIHandle.SetActive(false);

            state = UIState.VehicleSelection;

            onVehicleSelectionMode.Invoke();
        }


        /// <summary>
        /// Enter module selection mode.
        /// </summary>
        public virtual void EnterModuleSelection()
        {
            vehicleSelectionModeUIHandle.SetActive(false);
            moduleSelectionModeUIHandle.SetActive(true);
            state = UIState.ModuleSelection;

            onModuleSelectionMode.Invoke();
        }


        /// <summary>
        /// Cycle vehicle selection forward or back.
        /// </summary>
        /// <param name="forward">Whether to cycle forward.</param>
        public virtual void CycleVehicleSelection(bool forward)
        {
            if (loadoutManager.SlotPerVehicle)
            {
                loadoutManager.CycleSlot(forward, wrapVehicles);
            }
            else
            {
                loadoutManager.CycleVehicleSelection(forward, wrapVehicles);
            }
        }


        /// <summary>
        /// Called when a slot is clicked in the slot selection UI.
        /// </summary>
        /// <param name="ID">The ID of the slot (index in the loadout data slots list).</param>
        protected virtual void OnSlotClicked(int ID)
        {
            loadoutManager.SelectSlot(ID);
        }


        // Update the slots UI
        protected virtual void UpdateSlotsUI()
        {
            if (slotButtonsListController == null) return;

            if (!showSlots || loadoutManager.LoadoutData == null || loadoutManager.LoadoutData.Slots.Count < minVisibleSlots)
            {
                slotButtonsListController.SetNumButtons(0);
                if (slotSelectionUIHandle != null) slotSelectionUIHandle.gameObject.SetActive(false);
                return;
            }
            else
            {
                slotButtonsListController.SetNumButtons(loadoutManager.LoadoutData.Slots.Count);

                // Show as selected whatever slot is currently selected
                for (int i = 0; i < loadoutManager.LoadoutData.Slots.Count; ++i)
                {
                    slotButtonsListController.ButtonControllers[i].SetSelected(slotButtonsListController.ButtonControllers[i].ID ==
                                                                                loadoutManager.LoadoutData.Slots.IndexOf(loadoutManager.LoadoutData.SelectedSlot));

                    if (slotIconSpriteIndex != -1 && loadoutManager.LoadoutData.Slots[i].selectedVehicleIndex != -1 && loadoutManager.Items.vehicles[loadoutManager.LoadoutData.Slots[i].selectedVehicleIndex].sprites.Count > slotIconSpriteIndex)
                    {
                        slotButtonsListController.ButtonControllers[i].SetIcon(0, loadoutManager.Items.vehicles[loadoutManager.LoadoutData.Slots[i].selectedVehicleIndex].sprites[slotIconSpriteIndex]);
                        slotButtonsListController.ButtonControllers[i].SetText(0, loadoutManager.Items.vehicles[loadoutManager.LoadoutData.Slots[i].selectedVehicleIndex].Label);
                    }
                    else
                    {
                        slotButtonsListController.ButtonControllers[i].SetIcon(0, slotEmptyIcon);
                        slotButtonsListController.ButtonControllers[i].SetText(0, "EMPTY");
                    }
                }

                if (slotSelectionUIHandle != null) slotSelectionUIHandle.gameObject.SetActive(true);
            }
        }


        /// <summary>
        /// Called when a module is clicked in the module selection UI.
        /// </summary>
        /// <param name="ID">The ID of the module (index in the loadout manager's 'selectable module indexes' list).</param>
        protected virtual void OnModuleClicked(int ID)
        {
            loadoutManager.SelectModule(loadoutManager.SelectableModuleIndexes[ID]);
        }


        // Update the module selection UI
        protected virtual void UpdateModuleSelectionUI()
        {
            if (moduleButtonsListController == null) return;

            if (loadoutManager.WorkingSlot.selectedVehicleIndex == -1)
            {
                moduleButtonsListController.SetNumButtons(0);
                if (moduleOptionsUIHandle != null) moduleOptionsUIHandle.gameObject.SetActive(false);
                return;
            }
            else
            {
                // Update the number of buttons
                moduleButtonsListController.SetNumButtons(loadoutManager.SelectableModuleIndexes.Count);

                // Label and activate all the mount buttons
                for (int i = 0; i < loadoutManager.SelectableModuleIndexes.Count; ++i)
                {
                    LoadoutModuleItem moduleItem = loadoutManager.Items.modules[loadoutManager.SelectableModuleIndexes[i]];
                    moduleButtonsListController.ButtonControllers[i].SetSelected(loadoutManager.SelectedModuleMountIndex != -1 && loadoutManager.SelectableModuleIndexes[i] == loadoutManager.WorkingSlot.selectedModules[loadoutManager.SelectedModuleMountIndex]);
                    moduleButtonsListController.ButtonControllers[i].SetDeepSelected(loadoutManager.LoadoutData.SelectedSlot != null && loadoutManager.SelectedModuleMountIndex != -1 && loadoutManager.SelectableModuleIndexes[i] == loadoutManager.LoadoutData.SelectedSlot.selectedModules[loadoutManager.SelectedModuleMountIndex]);
                    moduleButtonsListController.ButtonControllers[i].SetText(0, moduleItem.Label);
                    moduleButtonsListController.ButtonControllers[i].SetIcon(0, moduleItem.sprites.Count > 0 ? moduleItem.sprites[0] : null);
                    moduleButtonsListController.ButtonControllers[i].gameObject.SetActive(true);
                }

                if (moduleOptionsUIHandle != null) moduleOptionsUIHandle.gameObject.SetActive(true);
            }
        }


        /// <summary>
        /// Called when a module mount is clicked in the module selection UI.
        /// </summary>
        /// <param name="ID">The index of the module mount in the vehicle's Module Mounts list.</param>
        protected virtual void OnModuleMountClicked(int ID)
        {
            loadoutManager.SelectModuleMount(ID);
        }


        // Update the module selection UI
        protected virtual void UpdateModuleMountSelectionUI()
        {
            if (moduleMountButtonsListController == null) return;

            if (loadoutManager.WorkingSlot.selectedVehicleIndex == -1)
            {
                moduleMountButtonsListController.SetNumButtons(0);
                if (moduleMountOptionsUIHandle != null) moduleMountOptionsUIHandle.gameObject.SetActive(false);
                return;
            }
            else
            {
                // Update the number of weapon mount buttons
                List<ModuleMount> moduleMounts = loadoutManager.Items.vehicles[loadoutManager.WorkingSlot.selectedVehicleIndex].vehiclePrefab.ModuleMounts;
                moduleMountButtonsListController.SetNumButtons(moduleMounts.Count);

                // Label and activate all the mount buttons
                for (int i = 0; i < moduleMounts.Count; ++i)
                {
                    moduleMountButtonsListController.ButtonControllers[i].SetSelected(loadoutManager.SelectedModuleMountIndex == i);

                    moduleMountButtonsListController.ButtonControllers[i].SetText(0, moduleMounts[i].Label);

                    if (loadoutManager.LoadoutData.SelectedSlot.selectedModules[i] != -1)
                    {
                        LoadoutModuleItem module = loadoutManager.Items.modules[loadoutManager.LoadoutData.SelectedSlot.selectedModules[i]];
                        moduleMountButtonsListController.ButtonControllers[i].SetText(1, module.Label);
                    }
                    else
                    {
                        moduleMountButtonsListController.ButtonControllers[i].SetText(1, "None");
                    }

                    moduleMountButtonsListController.ButtonControllers[i].gameObject.SetActive(true);
                }

                if (moduleMountOptionsUIHandle != null) moduleMountOptionsUIHandle.gameObject.SetActive(true);
            }
        }


        // Called when something changes on the loadout, to update the UI
        protected virtual void OnLoadoutChanged()
        {

            if (loadoutManager.Items == null) EnterVehicleSelection();


            // Update the slots menu

            UpdateSlotsUI();


            // Update the module selection UI

            UpdateModuleSelectionUI();


            // Update the module mount selection UI

            UpdateModuleMountSelectionUI();


            // Activate/deactivate the go-to-loadout (go to module selection) button

            if (goToLoadoutButton != null)
            {
                goToLoadoutButton.SetActive(loadoutManager.LoadoutData.SelectedSlot != null &&
                                    loadoutManager.WorkingSlot.selectedVehicleIndex != -1 &&
                                    loadoutManager.WorkingSlot.selectedVehicleIndex == loadoutManager.LoadoutData.SelectedSlot.selectedVehicleIndex);
            }


            // Activate/deactivate the equip vehicle button

            if (equipVehicleButton != null)
            {
                equipVehicleButton.SetActive(loadoutManager.LoadoutData.SelectedSlot != null &&
                                            loadoutManager.WorkingSlot.selectedVehicleIndex != -1 &&
                                            loadoutManager.WorkingSlot.selectedVehicleIndex != loadoutManager.LoadoutData.SelectedSlot.selectedVehicleIndex);
            }


            // Activate/deactivate the equip module button

            if (equipModuleButton != null)
            {
                equipModuleButton.SetActive(loadoutManager.LoadoutData.SelectedSlot != null && loadoutManager.LoadoutData.SelectedSlot.selectedVehicleIndex != -1 && loadoutManager.SelectedModuleMountIndex != -1 &&
                                        loadoutManager.LoadoutData.SelectedSlot.selectedModules[loadoutManager.SelectedModuleMountIndex] !=
                                        loadoutManager.WorkingSlot.selectedModules[loadoutManager.SelectedModuleMountIndex]);
            }


            // Activate/deactivate the next vehicle selection button

            if (selectNextVehicleButton != null)
            {
                if (loadoutManager.SlotPerVehicle)
                {
                    selectNextVehicleButton.SetActive(loadoutManager.LoadoutData.Slots.Count > 1 &&
                                                        (wrapVehicles || loadoutManager.LoadoutData.selectedSlotIndex < loadoutManager.LoadoutData.Slots.Count - 1));

                }
                else
                {
                    selectNextVehicleButton.SetActive(loadoutManager.SelectableVehicleIndexes.Count > 1 &&
                                                        (wrapVehicles || loadoutManager.SelectableVehicleIndexes.IndexOf(loadoutManager.WorkingSlot.selectedVehicleIndex) < loadoutManager.SelectableVehicleIndexes.Count - 1));

                }
            }


            // Activate/deactivate the previous vehicle selection button

            if (selectPreviousVehicleButton != null)
            {
                if (loadoutManager.SlotPerVehicle)
                {
                    selectPreviousVehicleButton.SetActive(loadoutManager.LoadoutData.Slots.Count > 1 &&
                                                        (wrapVehicles || loadoutManager.LoadoutData.selectedSlotIndex > 0));
                }
                else
                {
                    selectPreviousVehicleButton.SetActive(loadoutManager.SelectableVehicleIndexes.Count > 1 &&
                                                        (wrapVehicles || loadoutManager.SelectableVehicleIndexes.IndexOf(loadoutManager.WorkingSlot.selectedVehicleIndex) > 0));

                }
            }


            // Activate/deactivate the vehicle info

            if (vehicleInfoUIHandle != null)
            {
                vehicleInfoUIHandle.SetActive(loadoutManager.WorkingSlot.selectedVehicleIndex != -1);
            }


            // Activate/deactivate the module info

            if (moduleInfoUIHandle != null)
            {
                moduleInfoUIHandle.SetActive(loadoutManager.WorkingSlot.selectedVehicleIndex != -1);
            }
        }


        /// <summary>
        /// Clear the selected slot.
        /// </summary>
        public virtual void ClearSelectedSlot()
        {
            loadoutManager.ClearSelectedSlot();
        }


        /// <summary>
        /// Called by the buttons to equip/save a vehicle selection
        /// </summary>
        public virtual void EquipVehicle()
        {
            loadoutManager.SaveWorkingToActiveSlot();
        }


        /// <summary>
        /// Called by the buttons to equip/save a module selection
        /// </summary>
        public virtual void EquipModule()
        {
            loadoutManager.SaveWorkingToActiveSlot();
        }


        /// <summary>
        /// Clear the mounted module from the selected module mount.
        /// </summary>
        public virtual void ClearSelectedModuleMount()
        {
            loadoutManager.SelectModule(-1);
            loadoutManager.SaveWorkingToActiveSlot();
        }


        /// <summary>
        /// Go to the main menu.
        /// </summary>
        public virtual void MainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }


        /// <summary>
        /// Start a mission.
        /// </summary>
        /// <param name="index">The mission index in the Mission Scene Names list.</param>
        public virtual void StartMission(int index)
        {
            if (missionSceneNames.Count > index)
            {
                loadoutManager.SavePersistentData();
                SceneManager.LoadScene(missionSceneNames[index]);
            }
        }
    }
}
