using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class provides an example control script for a character.
    /// </summary>
    public class PlayerInput_InputSystem_CharacterControls : VehicleInput
    {

        [Header("Settings")]

        public float lookSensitivity = 0.1f;

        protected CharacterInputAsset input;
        protected GeneralInputAsset generalInput;
        protected Vector2 movement;
        protected Vector2 look;
        protected float run;

        protected GimbalController gimbalController;
        protected FirstPersonCharacterController characterController;


        private void OnEnable()
        {
            input.Enable();
            generalInput.Enable();
        }

        private void OnDisable()
        {
            input.Disable();
            generalInput.Enable();
        }

        protected override void Awake()
        {
            base.Awake();

            input = new CharacterInputAsset();
            generalInput = new GeneralInputAsset();

            // Steering
            input.CharacterControls.Move.performed += ctx => movement = ctx.ReadValue<Vector2>();
            input.CharacterControls.Jump.performed += ctx => Jump();
            input.CharacterControls.Run.performed += ctx => run = ctx.ReadValue<float>();
            //generalInput.GeneralControls.MouseDelta.performed += ctx => look = ctx.ReadValue<Vector2>();

        }


        /// <summary>
        /// Initialize this input script with a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns>Whether initialization succeeded</returns>
        protected override bool Initialize(Vehicle vehicle)
        {

            characterController = vehicle.GetComponent<FirstPersonCharacterController>();
            gimbalController = vehicle.GetComponent<GimbalController>();
            if (characterController == null)
            {
                if (debugInitialization)
                {
                    Debug.LogWarning(GetType().Name + " failed to initialize - the required " + typeof(CharacterController).Name + " component was not found on the vehicle.");
                }
                return false;
            }

            if (debugInitialization)
            {
                Debug.Log(GetType().Name + " successfully initialized.");
            }

            return true;
        }


        protected virtual void Jump()
        {
            if (initialized)
            {
                characterController.Jump();
            }
        }


        // Update is called once per frame
        protected override void InputUpdate()
        {
            // Moving
            float horizontal = movement.x;
            float forward = movement.y;

            characterController.SetMovementInputs(horizontal, 0, forward);

            characterController.SetRunning(run > 0.9f);

            look = generalInput.GeneralControls.MouseDelta.ReadValue<Vector2>();
            gimbalController.Rotate(look.x * lookSensitivity, -look.y * lookSensitivity);

            look = Vector2.zero;
        }
    }
}