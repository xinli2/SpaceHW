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
    public class PlayerInput_InputSystem_GimballedVehicleControls : VehicleInput
    {

        [Header("Settings")]

        public float lookSensitivity = 0.1f;

        protected GeneralInputAsset generalInput;
        protected Vector2 look;
        
        protected GimbalController gimbalController;


        private void OnEnable()
        {
            generalInput.Enable();
        }

        private void OnDisable()
        {
            generalInput.Enable();
        }

        protected override void Awake()
        {
            base.Awake();

            generalInput = new GeneralInputAsset();

            generalInput.GeneralControls.MouseDelta.performed += ctx => look = ctx.ReadValue<Vector2>();

        }


        /// <summary>
        /// Initialize this input script with a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>Whether initialization succeeded</returns>
        protected override bool Initialize(Vehicle vehicle)
        {

            gimbalController = vehicle.GetComponent<GimbalController>();
            if (gimbalController == null)
            {
                if (debugInitialization)
                {
                    Debug.LogWarning(GetType().Name + " failed to initialize - the required " + gimbalController.GetType().Name + " component was not found on the vehicle.");
                }
                return false;
            }

            if (debugInitialization)
            {
                Debug.Log(GetType().Name + " successfully initialized.");
            }

            return true;
        }


        protected override void InputUpdate()
        {
            gimbalController.Rotate(look.x * lookSensitivity, -look.y * lookSensitivity);

            look = Vector2.zero;
        }
    }
}