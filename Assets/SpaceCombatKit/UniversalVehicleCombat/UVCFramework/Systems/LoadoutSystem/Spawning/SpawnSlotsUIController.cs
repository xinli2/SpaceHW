using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.Utilities.UI;


namespace VSX.UniversalVehicleCombat.Loadout
{
    public class SpawnSlotsUIController : MonoBehaviour
    {
        [Tooltip("The component responsible for spawning the loadout vehicle.")]
        [SerializeField]
        protected LoadoutVehicleSpawner loadoutVehicleSpawner;

        [Tooltip("The controller for the loadout slots buttons.")]
        [SerializeField]
        protected ButtonsListController slotsButtons;

        [Tooltip("The gameobject to toggle to turn the UI on or off.")]
        [SerializeField]
        protected Transform activationHandle;

        [Tooltip("The minimum number of slots to show in the UI (since there is always at least one slot, for a single slot loadout there may be no point showing the UI).")]
        [SerializeField]
        protected int minVisibleSlots = 2;

        [Tooltip("The fill bar to show the progress of the spawn wait timer.")]
        [SerializeField]
        protected UIFillBarController spawnTimerFillBar;

        [Tooltip("The index of the sprite to show the vehicle on a slot button (the index within the Sprites list on the Loadout Vehicle Item for that vehicle).")]
        [SerializeField]
        protected int spriteIndex = 0;

        [Tooltip("The text to show the time remaining until spawn is completed.")]
        [SerializeField]
        protected UVCText spawnTimerText;

        [Tooltip("The preceeding message to display before the time value for the spawn timer text.")]
        [SerializeField]
        protected string spawnMessage = "SPAWNING IN";

        [Tooltip("The sprite to display for an empty slot.")]
        [SerializeField]
        protected Sprite emptySprite;

        [Tooltip("The label to display for an empty slot.")]
        [SerializeField]
        protected string emptyLabel = "EMPTY";


        protected virtual void Awake()
        {
            loadoutVehicleSpawner.onDataLoaded.AddListener(UpdateSlots);

            slotsButtons.onButtonClicked.AddListener(OnSlotsButtonClicked);
        }


        protected virtual void UpdateSlots()
        {
            if (loadoutVehicleSpawner.Data == null)
            {
                slotsButtons.SetNumButtons(0);
                return;
            }

            slotsButtons.SetNumButtons(loadoutVehicleSpawner.Data.Slots.Count);

            for (int i = 0; i < loadoutVehicleSpawner.Data.Slots.Count; ++i)
            {
                if (loadoutVehicleSpawner.Data.Slots[i].selectedVehicleIndex == -1)
                {
                    if (emptySprite != null) slotsButtons.ButtonControllers[i].SetIcon(0, emptySprite);
                    slotsButtons.ButtonControllers[i].SetText(0, emptyLabel);
                }
                else
                {
                    if (spriteIndex != -1)
                    {
                        slotsButtons.ButtonControllers[i].SetIcon(0, loadoutVehicleSpawner.LoadoutItems.vehicles[loadoutVehicleSpawner.Data.Slots[i].selectedVehicleIndex].sprites[spriteIndex]);
                    }

                    slotsButtons.ButtonControllers[i].SetText(0, loadoutVehicleSpawner.LoadoutItems.vehicles[loadoutVehicleSpawner.Data.Slots[i].selectedVehicleIndex].Label);
                }

                slotsButtons.ButtonControllers[i].SetSelected(i == loadoutVehicleSpawner.Data.selectedSlotIndex);

            }
        }


        protected virtual void OnSlotsButtonClicked(int ID)
        {
            loadoutVehicleSpawner.Data.selectedSlotIndex = ID;
            for (int i = 0; i < slotsButtons.ButtonControllers.Count; ++i)
            {
                slotsButtons.ButtonControllers[i].SetSelected(slotsButtons.ButtonControllers[i].ID == loadoutVehicleSpawner.Data.selectedSlotIndex);
            }
        }


        protected virtual void Update()
        {
            if (loadoutVehicleSpawner.Spawning)
            {
                activationHandle.gameObject.SetActive(true);
                spawnTimerFillBar.SetFillAmount(Mathf.Clamp((Time.time - loadoutVehicleSpawner.SpawnWaitStartTime) / loadoutVehicleSpawner.SpawnWaitTime, 0, 1));

                if (spawnTimerText != null)
                {
                    int timeRemaining = (int)Mathf.Ceil(loadoutVehicleSpawner.SpawnWaitTime - (Time.time - loadoutVehicleSpawner.SpawnWaitStartTime));
                    spawnTimerText.text = spawnMessage + " " + timeRemaining.ToString();
                }
            }
            else
            {
                activationHandle.gameObject.SetActive(false);
            }
        }
    }
}

