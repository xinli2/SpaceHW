using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Camera controls script.
    /// </summary>
    public class VehicleCameraInput : VehicleInput
    {

        [Header("Settings")]

        [SerializeField]
        protected CameraEntity cameraEntity;

        protected CameraTarget cameraTarget;

        protected GimbalController cameraGimbalController;

        protected VehicleEngines3D engines;

        [Header("Cycle Camera View")]

        [SerializeField]
        protected CustomInput cycleViewForwardInput = new CustomInput("Camera Controls", "Cycle camera view forward.", KeyCode.RightBracket);

        [SerializeField]
        protected CustomInput cycleViewBackwardInput = new CustomInput("Camera Controls", "Cycle camera view backward.", KeyCode.LeftBracket);

        [Header("Select Camera View")]

        [SerializeField]
        protected List<CameraViewInput> cameraViewInputs = new List<CameraViewInput>();

        [Header("Free Look Mode")]

        [SerializeField]
        protected float freeLookSpeed = 1;

        [SerializeField]
        protected bool disableEngineControlsInFreeLookMode = true;

        [SerializeField]
        protected bool clearSteeringInputsInFreeLookMode = true;

        [SerializeField]
        protected List<CameraView> freeLookModeCameraViews = new List<CameraView>();

        [SerializeField]
        protected CustomInput freeLookModeInput = new CustomInput("Camera Controls", "Free look mode.", KeyCode.LeftShift);

        [SerializeField]
        protected CustomInput lookHorizontalInput = new CustomInput("Camera Controls", "Free look horizontal.", "Mouse X");

        [SerializeField]
        protected CustomInput lookVerticalInput = new CustomInput("Camera Controls", "Free look vertical.", "Mouse Y");

        protected bool isFreeLookMode = false;


        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        protected void OnCameraFollowingVehicle(CameraEntity cameraEntity)
        {
            this.cameraEntity = cameraEntity;
            this.cameraGimbalController = cameraEntity.GetComponent<GimbalController>();
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
                // Link to camera already following
                if (cameraTarget.CameraEntity != null)
                {
                    OnCameraFollowingVehicle(cameraTarget.CameraEntity);
                }

                // Link to any new camera following
                cameraTarget.onCameraEntityTargeting.AddListener(OnCameraFollowingVehicle);
            }

            engines = vehicle.GetComponent<VehicleEngines3D>();

            return true;
        }


        protected virtual void OnFreeLookModeEngaged()
        {
            if (isFreeLookMode) return;

            if (freeLookModeCameraViews.Count > 0 && freeLookModeCameraViews.IndexOf(cameraEntity.CurrentView) == -1) return;

            isFreeLookMode = true;

            if (engines != null)
            {
                if (clearSteeringInputsInFreeLookMode) engines.SetSteeringInputs(Vector3.zero);
                if (disableEngineControlsInFreeLookMode) engines.ControlsDisabled = true;
            }
        }


        protected virtual void OnFreeLookModeDisengaged()
        {
            if (!isFreeLookMode) return;

            isFreeLookMode = false;

            if(cameraGimbalController != null) cameraGimbalController.ResetGimbal(true);

            if (engines != null)
            {
                if (disableEngineControlsInFreeLookMode) engines.ControlsDisabled = false;
            }
        }


        protected override void InputUpdate()
        {

            // Cycle camera view
            if (cameraEntity != null)
            {
                if (cycleViewForwardInput.Down())
                {
                    cameraEntity.CycleCameraView(true);
                }
                else if (cycleViewBackwardInput.Down())
                {
                    cameraEntity.CycleCameraView(false);
                }

                // Select camera view
                for (int i = 0; i < cameraViewInputs.Count; ++i)
                {
                    if (cameraViewInputs[i].input.Down())
                    {
                        cameraEntity.SetView(cameraViewInputs[i].view);
                    }
                }
            }

            if (freeLookModeInput.Down())
            {
                OnFreeLookModeEngaged();
            }

            if (freeLookModeInput.Up())
            {
                OnFreeLookModeDisengaged();
            }

            // Free look mode
            if (isFreeLookMode && cameraGimbalController != null)
            {
                cameraGimbalController.Rotate(lookHorizontalInput.FloatValue() * freeLookSpeed,
                                                            -lookVerticalInput.FloatValue() * freeLookSpeed);

                if (engines != null)
                {
                    if (clearSteeringInputsInFreeLookMode) engines.SetSteeringInputs(Vector3.zero);
                    if (disableEngineControlsInFreeLookMode) engines.ControlsDisabled = true;
                }
            }
        }
    }
}