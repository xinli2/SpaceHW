using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class provides an example control script for a space fighter.
    /// </summary>
    public class PlayerInput_InputSystem_EnterExitControls : VehicleInput
    {
        
        [Header("Enter/Exit Settings")]

        [SerializeField]
        protected GameAgent gameAgent;

        [SerializeField]
        protected bool prioritizeExiting = true;

        // The dependencies on the current vehicle
        protected VehicleEnterExitManager vehicleEnterExitManager;

        [SerializeField]
        protected bool setEnterExitPrompts = true;

        protected GeneralInputAsset input;
       

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

            input = new GeneralInputAsset();

            // Steering
            input.GeneralControls.Use.performed += ctx => EnterExit();
        }


        protected virtual void Reset()
        {
            gameAgent = transform.root.GetComponentInChildren<GameAgent>();
        }

        /// <summary>
        /// Initialize the vehicle input component.
        /// </summary>
        /// <param name="vehicle">The vehicle to intialize for.</param>
        /// <returns>Whether initialization succeeded.</returns>
        protected override bool Initialize(Vehicle vehicle)
        {
            // Update the dependencies
            vehicleEnterExitManager = vehicle.GetComponentInChildren<VehicleEnterExitManager>();
            if (vehicleEnterExitManager == null)
            {
                if (debugInitialization)
                {
                    Debug.LogWarning(GetType().Name + " failed to initialize - the required " + vehicleEnterExitManager.GetType().Name + " component was not found on the vehicle.");
                }
                return false;
            }

            if (debugInitialization)
            {
                Debug.Log(GetType().Name + " successfully initialized.");
            }

            return true;

        }


        bool CanRunInput()
        {
            return (initialized && inputEnabled && inputUpdateConditions.ConditionsMet);
        }


        // Called every frame
        protected void EnterExit()
        {
            if (!CanRunInput()) return;

            if (setEnterExitPrompts)
            {
                vehicleEnterExitManager.SetPrompts("PRESS F / A-BUTTON TO ENTER",
                                            "PRESS F / A-BUTTON TO EXIT");
            }

            if (prioritizeExiting)
            {
                if (vehicleEnterExitManager.CanExitToChild())
                {
                    Vehicle child = vehicleEnterExitManager.Child.Vehicle;
                    vehicleEnterExitManager.ExitToChild();
                    gameAgent.EnterVehicle(child);
                }
                else if (vehicleEnterExitManager.EnterableVehicles.Count > 0)
                {
                    Vehicle parent = vehicleEnterExitManager.EnterableVehicles[0].Vehicle;
                    vehicleEnterExitManager.EnterParent(0);
                    gameAgent.EnterVehicle(parent);
                }
            }
            else
            {
                if (vehicleEnterExitManager.EnterableVehicles.Count > 0)
                {
                    // Check for input
                    Vehicle parent = vehicleEnterExitManager.EnterableVehicles[0].Vehicle;
                    vehicleEnterExitManager.EnterParent(0);
                    gameAgent.EnterVehicle(parent);
                }
                else if (vehicleEnterExitManager.CanExitToChild())
                {
                    // Check for input
                    Vehicle child = vehicleEnterExitManager.Child.Vehicle;
                    vehicleEnterExitManager.ExitToChild();
                    gameAgent.EnterVehicle(child);
                }
            }
        }

        protected override void InputUpdate()
        {
            if (setEnterExitPrompts)
            {
                vehicleEnterExitManager.SetPrompts("PRESS F / A-BUTTON TO ENTER",
                                            "PRESS F / A-BUTTON TO EXIT");
            }
        }
    }
}