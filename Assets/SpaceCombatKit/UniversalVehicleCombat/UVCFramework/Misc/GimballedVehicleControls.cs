using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Controls a gimbal on a vehicle (a rotating joint such as a turret or mech torso).
    /// </summary>
    public class GimballedVehicleControls : VehicleInput
    {

        [Header("Settings")]

        [Tooltip("The gimballed vehicle controller that this input script will send input to.")]
        [SerializeField]
        protected GimballedVehicleController gimballedVehicleController;

        [Tooltip("How much smoothness is applied to the rotation of the vehicle's gimbal when input is applied.")]
        [SerializeField]
        protected float rotationSmoothing;
      
        [Header("Inputs")]

        [Tooltip("The input for rotating the vehicle's gimbal horizontally.")]
        [SerializeField]
        protected CustomInput horizontalRotationInput;

        [Tooltip("The input for rotating the vehicle's gimbal vertically.")]
        [SerializeField]
        protected CustomInput verticalRotationInput;

        [Tooltip("Whether to invert the vertical input (e.g. when using a mouse).")]
        [SerializeField]
        protected bool invertVerticalInput = false;
        public bool InvertVerticalInput
        {
            get { return invertVerticalInput; }
            set { invertVerticalInput = value; }
        }

        // Store the previous frame's values so can smoothly lerp between previous and current values
        protected float lastHorizontalInputValue;
        protected float lastVerticalInputValue;
        

        protected void Reset()
        {
            // Create default input for horizontal gimbal rotation
            horizontalRotationInput = new CustomInput("Gimbal Controls", "Horizontal Rotation", "Mouse X");

            // Create default input for vertical gimbal rotation
            verticalRotationInput = new CustomInput("Gimbal Controls", "Vertical Rotation", "Mouse Y");
        }

        private void OnValidate()
        {
            // Make sure rotation smoothing never falls below zero to prevent divide-by-zero error.
            rotationSmoothing = Mathf.Max(rotationSmoothing, 0);
        }

        // Called by the game agent that this input script belongs to when it enters a vehicle.
        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle)) return false;

            gimballedVehicleController = vehicle.GetComponent<GimballedVehicleController>();

            return (gimballedVehicleController != null);
        }

        // Called every frame that this input script is active
        protected override void InputUpdate()
        {
            // Get the next horizontal and vertical inputs for the gimbal
            float horizontalInputValue = Mathf.Lerp(lastHorizontalInputValue, horizontalRotationInput.FloatValue(), 1 / (1 + rotationSmoothing));
            float verticalInputValue = Mathf.Lerp(lastVerticalInputValue, verticalRotationInput.FloatValue(), 1 / (1 + rotationSmoothing));

            // Rotate the gimbal
            gimballedVehicleController.SetRotationInputs(horizontalInputValue, -verticalInputValue * (invertVerticalInput ? -1 : 1));

            // Update the stored values for the next frame
            lastHorizontalInputValue = horizontalInputValue;
            lastVerticalInputValue = verticalInputValue;
        }
    }
}

