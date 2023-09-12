using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class provides an example control script for a space fighter.
    /// </summary>
    public class SCKPlayerInput_InputSystem_ShipLanderControls : VehicleInput
    {
        [Header("Settings")]

        protected ShipLander shipLander;
        protected HUDShipLander hudShipLander;

        protected SCKInputAsset input;
     

        private void OnEnable()
        {
            input.Enable();
        }

        private void OnDisable()
        {
            input.Disable();
        }

        protected override void Awake()
        {
            base.Awake();

            input = new SCKInputAsset();

            // Steering
            input.SpacefighterControls.LaunchLand.performed += ctx => LaunchLand();
        }


        bool CanRunInput()
        {
            return (initialized && inputEnabled && inputUpdateConditions.ConditionsMet);
        }


        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle)) return false;

            shipLander = vehicle.GetComponentInChildren<ShipLander>();

            hudShipLander = vehicle.GetComponentInChildren<HUDShipLander>();

            if (shipLander == null)
            {
                if (debugInitialization)
                {
                    Debug.LogWarning(GetType().Name + " failed to initialize - the required " + shipLander.GetType().Name + " component was not found on the vehicle.");
                }

                return false;
            }
            else
            {
                hudShipLander.SetPrompts("PRESS L / B-BUTTON TO LAUNCH",
                                            "PRESS L / B-BUTTON TO LAND");

                if (debugInitialization)
                {
                    Debug.Log(GetType().Name + " successfully initialized.");
                }

                return true;
            }
        }


        void LaunchLand()
        {
            if (!CanRunInput()) return;

            switch (shipLander.CurrentState)
            {
                case (ShipLander.ShipLanderState.Launched):

                    shipLander.Land();

                    break;

                case (ShipLander.ShipLanderState.Landed):

                    shipLander.Launch();

                    break;
            }
        }
    }
}
