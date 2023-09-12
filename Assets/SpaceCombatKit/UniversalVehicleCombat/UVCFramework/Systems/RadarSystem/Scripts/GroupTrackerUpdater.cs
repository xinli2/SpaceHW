using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Improve performance by only updating a specified number of trackers each frame.
    /// </summary>
    public class GroupTrackerUpdater : MonoBehaviour
    {
        [Tooltip("The number of trackers that may be updated in a single frame.")]
        [SerializeField]
        protected int trackerUpdatesPerFrame = 1;

        [Tooltip("Trackers to add to the group on start.")]
        [SerializeField]
        protected List<Tracker> startingTrackers = new List<Tracker>();

        [Tooltip("Game agents to link - vehicles they enter will have their Trackers added.")]
        [SerializeField]
        protected List<GameAgent> gameAgents = new List<GameAgent>();

        [Tooltip("Spawners to link - spawned vehicles will have their Trackers added.")]
        [SerializeField]
        protected List<PilotedVehicleSpawn> spawners = new List<PilotedVehicleSpawn>();

        protected List<Tracker> trackers = new List<Tracker>();

        protected int currentIndex = -1;


        protected virtual void Awake()
        {
            foreach(Tracker tracker in startingTrackers)
            {
                AddTracker(tracker);
            }

            foreach (GameAgent gameAgent in gameAgents)
            {
                gameAgent.onEnteredVehicle.AddListener(AddVehicle);
                gameAgent.onExitedVehicle.AddListener(RemoveVehicle);

                AddVehicle(gameAgent.Vehicle);
            }

            foreach (PilotedVehicleSpawn spawner in spawners)
            {
                spawner.onSpawned.AddListener(() => { OnSpawnerSpawned(spawner); });
            }
        }


        protected virtual void AddTracker(Tracker tracker)
        {
            if (trackers.IndexOf(tracker) == -1)
            {
                tracker.UpdateTargetsEveryFrame = false;
                trackers.Add(tracker);
            }
        }


        protected virtual void RemoveTracker(Tracker tracker)
        {
            trackers.Remove(tracker);
        }


        protected virtual void AddVehicle(Vehicle vehicle)
        {
            if (vehicle != null)
            {
                Tracker[] vehicleTrackers = vehicle.GetComponentsInChildren<Tracker>();
                foreach (Tracker tracker in vehicleTrackers)
                {
                    AddTracker(tracker);
                }
            }
        }


        protected virtual void RemoveVehicle(Vehicle vehicle)
        {
            if (vehicle != null)
            {
                Tracker[] vehicleTrackers = vehicle.GetComponentsInChildren<Tracker>();
                foreach (Tracker tracker in vehicleTrackers)
                {
                    RemoveTracker(tracker);
                }
            }
        }


        protected virtual void OnSpawnerSpawned(PilotedVehicleSpawn spawner)
        {
            spawner.Pilot.onEnteredVehicle.AddListener(AddVehicle);
            spawner.Pilot.onExitedVehicle.AddListener(RemoveVehicle);

            AddVehicle(spawner.Vehicle);
        }


        protected virtual void Update()
        {
            if (trackers.Count > 0 && trackerUpdatesPerFrame > 0)
            {
                currentIndex = Mathf.Clamp(currentIndex, 0, trackers.Count - 1);

                int startIndex, endIndex;
                if (trackerUpdatesPerFrame >= trackers.Count)
                {
                    startIndex = 0;
                    endIndex = trackers.Count - 1;
                }
                else
                {
                    startIndex = currentIndex;
                    endIndex = (currentIndex + (trackerUpdatesPerFrame - 1)) % trackers.Count;
                }

                for(int i = startIndex; i <= endIndex; ++i)
                {
                    trackers[i].UpdateTargets();
                }

                currentIndex = (endIndex + 1) % trackers.Count;
            }
        }
    }
}

