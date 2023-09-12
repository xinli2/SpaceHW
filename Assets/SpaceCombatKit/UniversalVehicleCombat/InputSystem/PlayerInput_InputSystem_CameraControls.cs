using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Input script for controlling the steering and movement of a space fighter vehicle.
    /// </summary>
    public class PlayerInput_InputSystem_CameraControls : VehicleInput
    {
        
        protected GeneralInputAsset input;

        protected CameraTarget cameraTarget;

        protected CameraEntity cameraEntity;


        protected override void Awake()
        {

            base.Awake();

            input = new GeneralInputAsset();

            input.CameraControls.CycleCameraViewForward.performed += ctx => CycleCameraView(true);

            input.CameraControls.CycleCameraViewBack.performed += ctx => CycleCameraView(false);

        }


        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle)) return false;

            // Unlink from previous camera target
            if (cameraTarget != null)
            {
                cameraTarget.onCameraEntityTargeting.RemoveListener(OnCameraFollowingVehicle);
            }

            // Link to camera following camera target
            cameraTarget = vehicle.GetComponent<CameraTarget>();

            if (cameraTarget != null)
            {
                // Link to any new camera following
                cameraTarget.onCameraEntityTargeting.AddListener(OnCameraFollowingVehicle);

                // Link to camera already following
                if (cameraTarget.CameraEntity != null)
                {
                    OnCameraFollowingVehicle(cameraTarget.CameraEntity);
                }
            }

            return true;
        }


        protected virtual void OnCameraFollowingVehicle(CameraEntity cameraEntity)
        {
            this.cameraEntity = cameraEntity;
        }


        protected virtual void CycleCameraView(bool forward)
        {
            if (!inputEnabled) return;
            
            if (cameraEntity != null)
            {
                cameraEntity.CycleCameraView(forward);
            }
        }


        protected virtual void OnEnable()
        {
            input.Enable();
        }


        protected virtual void OnDisable()
        {
            input.Disable();
        }
    }
}