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
    /// This class provides an example control script for a capital ship.
    /// </summary>
    public class SCKPlayerInput_InputSystem_CapitalShipControls : VehicleInput
    {

        [Header("Gimbal Rotation")]

        [Tooltip("How fast the camera rotates when looking around.")]
        [SerializeField]
        protected float lookRotationSpeed = 80;

        [Tooltip("The gimbal (joint) for the camera.")]
        [SerializeField]
        protected GimbalController gimbalController;
        protected Vector2 gimbalRotationInput;

        [Header("Throttle")]

        [Tooltip("How fast the throttle changes when accelerating or decelerating.")]
        [SerializeField]
        protected float throttleSensitivity = 0.5f;

        [Header("Pitch And Roll Correction")]

        [Tooltip("The PID controller for the auto levelling.")]
        [SerializeField]
        protected ShipPIDController shipPIDController;

        protected VehicleEngines3D engines;

        protected SCKInputAsset input;

        protected float acceleration, boost;
        protected Vector2 steering, strafing;


        protected virtual void OnEnable()
        {
            input.Enable();
        }


        protected virtual void OnDisable()
        {
            input.Disable();
        }


        protected override void Awake()
        {
            base.Awake();

            input = new SCKInputAsset();

            // Gimbal rotation
            input.CapitalShipControls.Look.performed += ctx => gimbalRotationInput = ctx.ReadValue<Vector2>();

            // Steering
            input.CapitalShipControls.Steer.performed += ctx => steering = ctx.ReadValue<Vector2>();

            // Strafing
            input.CapitalShipControls.Strafe.performed += ctx => strafing = ctx.ReadValue<Vector2>();

            // Acceleration
            input.CapitalShipControls.Throttle.performed += ctx => acceleration = ctx.ReadValue<float>();

            // Boost
            input.CapitalShipControls.Boost.performed += ctx => boost = ctx.ReadValue<float>();

        }


        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle)) return false;

            engines = vehicle.GetComponent<VehicleEngines3D>();
            gimbalController = vehicle.GetComponent<GimbalController>();

            if (engines == null)
            {
                if (debugInitialization)
                {
                    Debug.LogWarning(GetType().Name + " failed to initialize - the required " + engines.GetType().Name + " component was not found on the vehicle.");
                }

                return false;
            }

            if (debugInitialization)
            {
                Debug.Log(GetType().Name + " successfully initialized.");
            }

            return true;

        }


        // Set the control values for the vehicle
        protected virtual void SetControlValues()
        {

            // Values to be passed to the ship
            float pitch, yaw, roll;

            Vector3 flattenedForward = new Vector3(engines.transform.forward.x, 0f, engines.transform.forward.z).normalized;
            Maneuvring.TurnToward(engines.transform, engines.transform.position + flattenedForward, new Vector3(0f, 360f, 0f), shipPIDController.steeringPIDController);

            pitch = shipPIDController.steeringPIDController.GetControlValue(PIDController3D.Axis.X);
            roll = shipPIDController.steeringPIDController.GetControlValue(PIDController3D.Axis.Z);
            yaw = steering.x;

            // ************************** Throttle ******************************

            Vector3 nextMovementInputs = engines.MovementInputs;
            nextMovementInputs.z += acceleration * throttleSensitivity * Time.deltaTime;

            // Left / right movement
            nextMovementInputs.x = strafing.x;

            // Up / down movement
            nextMovementInputs.y = strafing.y;

            engines.SetMovementInputs(nextMovementInputs);

            engines.SetBoostInputs(new Vector3(0f, 0f, boost));

            engines.SetSteeringInputs(new Vector3(pitch, yaw, roll));

        }


        // Called every frame
        protected override void InputUpdate()
        {
            if (gimbalController != null)
            {
                Vector2 rotation = lookRotationSpeed * gimbalRotationInput * Time.deltaTime;
                gimbalController.Rotate(rotation.x, rotation.y);
                gimbalRotationInput = Vector2.zero;
            }

            SetControlValues();
        }
    }
}
