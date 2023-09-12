using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat.Loadout
{

    /// <summary>
    /// Example input system script for loadout controls.
    /// </summary>
    public class InputSystem_LoadoutControls : GeneralInput
    {

        [Tooltip("The loadout camera controller.")]
        [SerializeField]
        protected LoadoutCameraController loadoutCameraController;

        protected LoadoutInputAsset input;


        protected override void Awake()
        {
            base.Awake();

            input = new LoadoutInputAsset();

            input.LoadoutControls.Look.performed += ctx => Rotate(ctx.ReadValue<Vector2>());

            input.LoadoutControls.LookEngage.performed += ctx => BeginViewRotation();
            input.LoadoutControls.LookEngage.canceled += ctx => EndViewRotation();
        }


        protected virtual void OnEnable()
        {
            input.Enable();
        }


        protected virtual void OnDisable()
        {
            input.Disable();
        }


        // Begin view rotation
        protected virtual void BeginViewRotation()
        {
            loadoutCameraController.SetViewRotating(true);
        }


        // End view rotation
        protected virtual void EndViewRotation()
        {
            loadoutCameraController.SetViewRotating(false);
        }


        // Called when rotation inputs change
        protected virtual void Rotate(Vector3 inputs)
        {
            loadoutCameraController.SetViewRotationInputs(inputs);
        }
    }
}
