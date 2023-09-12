using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Unity event for running functions when the game agent enters a vehicle.
    /// </summary>
    [System.Serializable]
    public class OnGameAgentEnteredVehicleEventHandler : UnityEvent <Vehicle> { }

    /// <summary>
    /// Unity event for running functions when the game agent exits a vehicle
    /// </summary>
    [System.Serializable]
    public class OnGameAgentExitedVehicleEventHandler : UnityEvent<Vehicle> { }

    /// <summary>
    /// Unity event for running functions when the game agent dies. 
    /// </summary>
    [System.Serializable]
    public class OnGameAgentDiedEventHandler : UnityEvent { }

    /// <summary>
    /// Unity event for running functions when the game agent is revived.
    /// </summary>
    [System.Serializable]
    public class OnGameAgentRevivedEventHandler : UnityEvent { }


    /// <summary>
    /// A base class for a player or AI that can enter, exit and control vehicles.
    /// </summary>
    public class GameAgent : MonoBehaviour 
	{

        [Header("General")]

        [Tooltip("The name or label of the game agent.")]
        [SerializeField]
		protected string label;
		public virtual string Label { get { return label; } }

        [Tooltip("Whether this game agent is a Player or an NPC.")]
        [SerializeField]
        protected bool isPlayer = true;
        public virtual bool IsPlayer { get { return isPlayer; } }

        [Tooltip("The team that this game agent belongs to.")]
        [SerializeField]
        protected Team team;
        public virtual Team Team { get { return team; } }

        [Tooltip("The vehicle to enter when the scene starts.")]
        [SerializeField]
        protected Vehicle startingVehicle;

        // The vehicle that the game agent is currently in.
		protected Vehicle vehicle;
		public virtual Vehicle Vehicle { get { return vehicle; } }
	
        // Whether the game agent is in a vehicle or not.
		protected bool isInVehicle;
		public virtual bool IsInVehicle { get { return isInVehicle; } }

        // Whether the game agent is dead
        protected bool isDead = false;
        public virtual bool IsDead { get { return isDead; } }

        // List of all the input scripts for this game agent
        protected List<VehicleInput> vehicleInputs = new List<VehicleInput>();
        public virtual List<VehicleInput> VehicleInputs { get { return vehicleInputs; } }

        [SerializeField]
        protected Character character;
        public Character Character { get { return character; } }

        [Header("Events")]

        [Tooltip("Called when this game agent enters a vehicle.")]
        public OnGameAgentEnteredVehicleEventHandler onEnteredVehicle;

        [Tooltip("Called when this game agent exits a vehicle.")]
        public OnGameAgentExitedVehicleEventHandler onExitedVehicle;

        [Tooltip("Called when this game agent dies.")]
        public OnGameAgentDiedEventHandler onDied;

        [Tooltip("Called when this game agent is revived.")]
        public OnGameAgentRevivedEventHandler onRevived;



        // Called when the component is first added to a gameobject or is reset in the inspector.
        protected virtual void Reset()
        {
            label = "Player";
        }


        protected virtual void Awake()
        {
            if (GameAgentManager.Instance != null) GameAgentManager.Instance.Register(this);

            // Collect all the vehicle input components in the hierarchy
            vehicleInputs = new List<VehicleInput>(GetComponentsInChildren<VehicleInput>());
            for (int i = 0; i < vehicleInputs.Count; ++i)
            {
                int tempIndex = i;
                onDied.AddListener(() => vehicleInputs[tempIndex].SetVehicle(null));
            }

            if (character != null && character.gameObject.scene.rootCount == 0)  // Check that this is a valid way of checking for prefab
            {
                character = Instantiate(character, Vector3.zero, Quaternion.identity);
                character.gameObject.SetActive(false);
            }
        }


        // Called when the game starts
        protected virtual void Start()
		{
            // Enter the starting vehicle
            if (vehicle == null && startingVehicle != null)
			{
				EnterVehicle(startingVehicle);
			}
		}


        /// <summary>
		/// Make the game agent enter a new vehicle.
		/// </summary>
		/// <param name="newVehicle"> The vehicle being entered.</param>
        public virtual void EnterVehicle(Vehicle newVehicle)
		{
            Vehicle previousVehicle = vehicle;
            
            // Exit last vehicle
            if (isInVehicle)
			{				
                // Stop input scripts
                for (int i = 0; i < vehicleInputs.Count; ++i)
                {
                    vehicleInputs[i].SetVehicle(null);
                }

                // Exit the vehicle
                vehicle.OnExited(this);
                vehicle.onDestroyed.RemoveListener(Kill);

                // Call the event
                onExitedVehicle.Invoke(vehicle);
			}
            
            // Update the vehicle
			vehicle = newVehicle;
			isInVehicle = vehicle != null;

			// Enter new vehicle
			if (vehicle != null)
			{
                vehicle.OnEntered(this);
                vehicle.onDestroyed.AddListener(Kill);

                // Run compatible input scripts
                int numInputFound = 0;
                for (int i = 0; i < vehicleInputs.Count; ++i)
                {
                    vehicleInputs[i].SetVehicle(newVehicle);
                    if (vehicleInputs[i].Initialized)
                    {
                        numInputFound += 1;
                    }
                }
                
                if (numInputFound == 0)
                {
                    Debug.LogWarning("No compatible input scripts found for the " + vehicle.name + " vehicle in the " + name + " Game Agent's hierarchy. Please add one or more to control vehicle functionality.");
                }
            }

            if (newVehicle == character)
            {
                if (previousVehicle != null)
                {
                    if (previousVehicle.CharacterExitSpawn != null)
                    {
                        character.SetPositionAndRotation(previousVehicle.CharacterExitSpawn.position, previousVehicle.CharacterExitSpawn.rotation);
                    }
                }

                character.gameObject.SetActive(true);

            }
            else
            {
                if (character != null)
                {
                    character.gameObject.SetActive(false);
                }
            }

            // Call the event
            onEnteredVehicle.Invoke(vehicle);
        }


        public virtual void ExitAllVehicles()
        {
            EnterVehicle(null);
        }


        /// <summary>
        /// Called to kill the game agent (for example when the vehicle is destroyed)
        /// </summary>
        public virtual void Kill()
        {
            isDead = true;
            onDied.Invoke();
        }


        /// <summary>
        /// Called to reset the game agent after it dies.
        /// </summary>
        public virtual void Revive()
        {
            EnterVehicle(null);
            isDead = false;
            onRevived.Invoke();
        }
    }
}
