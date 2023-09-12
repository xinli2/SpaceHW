using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Provides a public function you can add to a Unity UI Slider event to change the throttle setting of the Vehicle Engines 3D component on a vehicle that the player is currently in.
    /// </summary>
    public class PlayerEnginesThrottleSliderController : MonoBehaviour
    {
        [Tooltip("The player in the scene. If using the Game Agent Manager, no need to set this, the player will be found at the scene start.")]
        [SerializeField]
        protected GameAgent player;

        protected VehicleEngines3D engines;



        protected virtual void Awake()
        {
            if (GameAgentManager.Instance != null) GameAgentManager.Instance.onFocusedGameAgentChanged.AddListener(SetPlayer);

            if (player != null)
            {
                SetPlayer(player);
            }
        }


        protected virtual void SetPlayer(GameAgent newPlayer)
        {
            if (player != null)
            {
                player.onEnteredVehicle.RemoveListener(OnVehicleChanged);
                player.onExitedVehicle.RemoveListener(OnVehicleChanged);
            }

            player = newPlayer;

            if (player != null)
            {
                if (player.Vehicle != null) OnVehicleChanged(player.Vehicle);
                player.onEnteredVehicle.AddListener(OnVehicleChanged);
                player.onExitedVehicle.AddListener(OnVehicleChanged);
            }
        }


        protected virtual void OnVehicleChanged(Vehicle vehicle)
        {
            if (player.Vehicle == null)
            {
                engines = null;
            }
            else
            {
                engines = player.Vehicle.GetComponentInChildren<VehicleEngines3D>();
            }
        }


        /// <summary>
        /// Set the Vehicle Engines 3D throttle z-value.
        /// </summary>
        /// <param name="value">The throttle z-value.</param>
        public virtual void SetThrottle(float value)
        {
            if (engines != null)
            {
                Vector3 movementInputs = engines.MovementInputs;
                movementInputs.z = value;
                engines.SetMovementInputs(movementInputs);
            }
        }
    }
}

