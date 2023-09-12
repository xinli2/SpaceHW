using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    public class GameAgentVehicleSetupManager : ModuleManager
    {

        [SerializeField]
        protected Vehicle vehicle;

        public UnityEvent onPlayerEnteredVehicle;

        public UnityEvent onAIEnteredVehicle;

        public UnityEvent onVehicleExited;

        protected List<Trackable> trackables = new List<Trackable>();
        protected TargetSelector weaponTargetSelector;

        protected Trackable rootTrackable;

        public enum TargetLabelOverride
        {
            None,
            GameAgent,
            Vehicle
        }

        [Tooltip("Label that appears on radar/target boxes when this vehicle is tracked. Can show Game Agent name, Vehicle name, or leave unchanged.")]
        [SerializeField]
        protected TargetLabelOverride labelOverrideType;

        [SerializeField]
        protected string labelKey = "Label";

        protected string originalLabel = "";
        protected Team originalTeam;

        protected bool dataInitialized = false;


        protected virtual void Reset()
        {
            vehicle = transform.GetComponent<Vehicle>();
        }


        protected override void Awake()
        {
            base.Awake();

            trackables = new List<Trackable>(transform.GetComponentsInChildren<Trackable>());

            rootTrackable = GetComponent<Trackable>();

            Weapons weapons = transform.GetComponentInChildren<Weapons>();
            if (weapons != null)
            {
                weaponTargetSelector = weapons.WeaponsTargetSelector;
            }
            
            vehicle.onGameAgentEntered.AddListener(OnVehicleEntered);
            vehicle.onGameAgentExited.AddListener(OnVehicleExited);


        }

        protected override void Start()
        {
            base.Start();

            InitializeData();

            UpdateRootTrackableLabel();
        }


        protected virtual void InitializeData()
        {
            if (trackables.Count > 0)
            {
                originalTeam = trackables[0].Team;

                if (trackables[0].variablesDictionary.ContainsKey(labelKey))
                {
                    LinkableVariable labelVariable = trackables[0].variablesDictionary[labelKey];
                    if (labelVariable != null)
                    {
                        originalLabel = labelVariable.StringValue;
                    }
                }
            }

            dataInitialized = true;
        }


        protected override void OnModuleMounted(Module module)
        {
            base.OnModuleMounted(module);

            Trackable[] moduleTrackables = module.GetComponentsInChildren<Trackable>();
            foreach(Trackable trackable in moduleTrackables)
            {
                trackables.Add(trackable);
                trackable.SetParentTrackable(rootTrackable);
            }

            UpdateTrackables();
        }


        protected override void OnModuleUnmounted(Module module)
        {
            base.OnModuleUnmounted(module);

            Trackable[] moduleTrackables = module.GetComponentsInChildren<Trackable>();
            foreach (Trackable trackable in moduleTrackables)
            {
                trackable.SetParentTrackable(null);
                trackables.Remove(trackable);
            }
        }


        protected virtual void UpdateRootTrackableLabel()
        {
            if (rootTrackable == null) return;

            // Update the label on the root trackable
            string label = originalLabel;
            switch (labelOverrideType)
            {
                case TargetLabelOverride.GameAgent:

                    if (vehicle.Occupants.Count > 0) label = vehicle.Occupants[0].Label;
                    break;

                case TargetLabelOverride.Vehicle:

                    label = vehicle.Label;
                    break;

            }

            if (rootTrackable.variablesDictionary.ContainsKey(labelKey))
            {
                LinkableVariable labelVariable = rootTrackable.variablesDictionary[labelKey];
                if (labelVariable != null)
                {
                    labelVariable.StringValue = label;
                }
            }
        }


        protected virtual void UpdateTrackables()
        {
            // Update the vehicle's team
            Team team = vehicle.Occupants.Count > 0 ? vehicle.Occupants[0].Team : originalTeam;

            // Update the Team for all the trackables on this vehicle
            for (int i = 0; i < trackables.Count; ++i)
            {
                trackables[i].Team = team;
            }
        }


        protected virtual void UpdateTargetSelectors()
        {
            // Update the vehicle's team
            Team team = vehicle.Occupants.Count > 0 ? vehicle.Occupants[0].Team : originalTeam;

            if (team != null)
            {
                if (weaponTargetSelector != null)
                {
                    weaponTargetSelector.SelectableTeams = team.HostileTeams;
                }
            }
        }

        public virtual void OnVehicleEntered(GameAgent gameAgent)
        {
            if (!dataInitialized)
            {
                InitializeData();
            }

            UpdateTrackables();
            UpdateTargetSelectors();

            UpdateRootTrackableLabel();

            // Call the event
            if (gameAgent != null)
            {
                if (gameAgent.IsPlayer)
                {
                    onPlayerEnteredVehicle.Invoke();
                }
                else
                {
                    onAIEnteredVehicle.Invoke();
                }
            }
        }

        protected virtual void OnVehicleExited(GameAgent gameAgent)
        {
            UpdateTrackables();
            UpdateTargetSelectors();
            onVehicleExited.Invoke();
        }
    }
}

