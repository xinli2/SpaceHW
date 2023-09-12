using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat.Loadout
{
    public class LoadoutVehicleSpawner : MonoBehaviour
    {
        [Tooltip("The loadout data manager to load the vehicle and loadout information from.")]
        [SerializeField]
        protected LoadoutDataManager loadoutDataManager;

        protected LoadoutData data;
        public LoadoutData Data { get { return data; } }

        [Tooltip("The loadout items (vehicles and modules) associated with the saved loadout data.")]
        [SerializeField]
        protected LoadoutItems loadoutItems;
        public LoadoutItems LoadoutItems { get { return loadoutItems; } }

        [Tooltip("The player in the scene.")]
        [SerializeField]
        protected GameAgent player;

        [SerializeField]
        protected bool enterVehicleImmediate = true;

        [Tooltip("The vehicle to load if there is no loadout data found.")]
        [SerializeField]
        protected Vehicle backupVehiclePrefab;

        [Tooltip("Whether to load the selected slot, or just the first available one.")]
        [SerializeField]
        protected bool loadSelectedSlot = true;

        protected bool spawning = false;
        public bool Spawning { get { return spawning; } }

        protected float spawnWaitStartTime;
        public float SpawnWaitStartTime { get { return spawnWaitStartTime; } }

        [Tooltip("How long until the player vehicle spawns after the spawning process begins.")]
        [SerializeField]
        protected float spawnWaitTime = 0;
        public float SpawnWaitTime { get { return spawnWaitTime; } }

        [Tooltip("Whether to spawn the vehicle when the scene starts.")]
        [SerializeField]
        protected bool spawnOnStart = true;

        [Tooltip("The game state to enter when the spawning process starts.")]
        [SerializeField]
        protected GameState spawningState;

        [Tooltip("The game state to enter when the spawning process is complete.")]
        [SerializeField]
        protected GameState spawnedState;

        [Tooltip("Event called when the loadout data is loaded.")]
        public UnityEvent onDataLoaded;

        [Tooltip("Event called when the spawning process starts.")]
        public UnityEvent onSpawnStarted;

        [Tooltip("Event called when spawning is complete.")]
        public UnityEvent onSpawned;

        public VehicleEvent onVehicleSpawned;

        


        protected virtual void Start()
        {
            if (spawnOnStart)
            {
                Spawn();
            }
        }


        /// <summary>
        /// Spawn the loadout vehicle.
        /// </summary>
        public virtual void Spawn()
        {
            if (data == null)
            {
                LoadData();
            }

            StartCoroutine(SpawnCoroutine());
            
        }


        IEnumerator SpawnCoroutine()
        {
            // Waiting for spawn

            onSpawnStarted.Invoke();

            if (!Mathf.Approximately(spawnWaitTime, 0))
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.EnterGameState(spawningState);
                }

                spawnWaitStartTime = Time.time;

                spawning = true;

                yield return new WaitForSeconds(spawnWaitTime);
            }

            // Spawn loadout vehicle
            Vehicle spawnedVehicle = null;
            if (data != null)
            {
                int vehicleIndex = data.SelectedSlot.selectedVehicleIndex;
                if (vehicleIndex != -1)
                {
                    spawnedVehicle = Instantiate(loadoutItems.vehicles[data.SelectedSlot.selectedVehicleIndex].vehiclePrefab, transform.position, transform.rotation);

                    foreach (ModuleMount moduleMount in spawnedVehicle.ModuleMounts)
                    {
                        moduleMount.createDefaultModulesAtStart = false;
                    }

                    for (int i = 0; i < data.SelectedSlot.selectedModules.Count; ++i)
                    {
                        int moduleIndex = data.SelectedSlot.selectedModules[i];
                        if (moduleIndex != -1)
                        {
                            Module module = Instantiate(loadoutItems.modules[data.SelectedSlot.selectedModules[i]].modulePrefab, transform.position, transform.rotation);
                            spawnedVehicle.ModuleMounts[i].AddModule(module, true);
                        }
                    } 
                }
            }

            // Spawn backup vehicle
            if (spawnedVehicle == null && backupVehiclePrefab != null)
            {
                spawnedVehicle = Instantiate(backupVehiclePrefab, transform.position, transform.rotation);
            }

            // Enter vehicle
            if (spawnedVehicle != null && enterVehicleImmediate) player.EnterVehicle(spawnedVehicle);

            onSpawned.Invoke();
            onVehicleSpawned.Invoke(spawnedVehicle);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.EnterGameState(spawnedState);
            }

            spawning = false;
        }

        protected virtual void LoadData()
        {
            if (loadoutDataManager != null) data = loadoutDataManager.LoadData();

            VerifyLoadoutData();

            onDataLoaded.Invoke();
        }

        protected virtual void VerifyLoadoutData()
        {
            if (data == null) return;

            if (data.Slots.Count > 0)
            {
                // Verify the vehicles

                for (int i = 0; i < data.Slots.Count; ++i)
                {
                    if (data.Slots[i].selectedVehicleIndex != -1)
                    {
                        // If the vehicle index is outside the range of the vehicles list, clear it
                        if (data.Slots[i].selectedVehicleIndex >= loadoutItems.vehicles.Count)
                        {
                            data.Slots[i].selectedVehicleIndex = -1;
                        }
                    }
                }

                // Verify the number of module mounts

                foreach (LoadoutSlot slot in data.Slots)
                {
                    if (slot.selectedVehicleIndex != -1)
                    {
                        int numModuleMountsOnVehicle = loadoutItems.vehicles[slot.selectedVehicleIndex].vehiclePrefab.ModuleMounts.Count;
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

                for (int i = 0; i < data.Slots.Count; ++i)
                {
                    if (data.Slots[i].selectedVehicleIndex == -1) continue;

                    for (int j = 0; j < data.Slots[i].selectedModules.Count; ++j)
                    {
                        if (data.Slots[i].selectedModules[j] == -1) continue;

                        if (data.Slots[i].selectedModules[j] >= loadoutItems.modules.Count)
                        {
                            data.Slots[i].selectedModules[j] = -1;
                            continue;
                        }
                    }
                }

                // Verify the selected slot
                if (data.Slots.Count == 0)
                {
                    data.selectedSlotIndex = -1;
                }
                else
                {
                    if (loadSelectedSlot)
                    {
                        if (data.selectedSlotIndex < 0 || data.selectedSlotIndex >= data.Slots.Count)
                        {
                            data.selectedSlotIndex = 0;
                        }
                    }
                    else
                    {
                        data.selectedSlotIndex = 0;
                    }
                }
            }
        }
    }
}

