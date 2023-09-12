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
    public class PlayerInput_InputSystem_CameraFreeLookControls : VehicleInput
    {
        
        protected GeneralInputAsset input;

        protected CameraTarget cameraTarget;

        protected CameraEntity cameraEntity;

        protected GimbalController cameraGimbalController;

        [Header("Free Look")]

        [Tooltip("How fast the camera rotates in free look mode.")]
        [SerializeField]
        protected float freeLookSpeed = 0.1f;

        [Tooltip("Whether to invert the vertical axis of the camera rotation in free look mode.")]
        [SerializeField]
        protected bool invertFreeLookVertical = false;

        protected bool isFreeLookMode = false;

        [SerializeField]
        protected List<CameraView> freeLookModeCameraViews = new List<CameraView>();

        [SerializeField]
        protected GameState freeLookEnterGameState;

        [SerializeField]
        protected GameState freeLookExitGameState;



        protected override void Awake()
        {

            base.Awake();

            input = new GeneralInputAsset();

            input.CameraControls.FreeLookMode.started += ctx => EnterFreeLookMode();

            input.CameraControls.FreeLookMode.canceled += ctx => ExitFreeLookMode();

        }


        protected override bool Initialize(Vehicle vehicle)
        {
            isFreeLookMode = false;
            if (cameraGimbalController != null)
            {
                cameraGimbalController.ResetGimbal(true);
            }

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
            
            return true;
        }

        protected virtual void OnCameraFollowingVehicle(CameraEntity cameraEntity)
        {
            ExitFreeLookMode();

            cameraGimbalController = null;

            if (cameraEntity != null)
            {
                cameraEntity.onCameraViewTargetChanged.RemoveListener(OnCameraViewChanged);
            }

            this.cameraEntity = cameraEntity;

            if (cameraEntity != null)
            {
                cameraEntity.onCameraViewTargetChanged.AddListener(OnCameraViewChanged);
                cameraGimbalController = cameraEntity.GetComponent<GimbalController>();
            }
        }


        protected virtual void OnCameraViewChanged(CameraViewTarget cameraViewTarget)
        {
            ExitFreeLookMode();
        }


        protected virtual void FreeLookModeUpdate()
        {

            if (!isFreeLookMode) return;

            // Free look mode
            if (cameraGimbalController != null)
            {
                cameraGimbalController.Rotate(Mouse.current.delta.x.ReadValue() * freeLookSpeed, -Mouse.current.delta.y.ReadValue() * (invertFreeLookVertical ? -1 : 1) * freeLookSpeed);
            }
        }


        protected virtual void EnterFreeLookMode()
        {
            if (!inputEnabled) return;

            if (isFreeLookMode) return;

            if (cameraEntity == null) return;

            if (freeLookModeCameraViews.Count != 0 && freeLookModeCameraViews.IndexOf(cameraEntity.CurrentView) == -1) return;

            isFreeLookMode = true;

            if (GameStateManager.Instance != null && freeLookEnterGameState != null)
            {
                GameStateManager.Instance.EnterGameState(freeLookEnterGameState);
            }
        }


        protected virtual void ExitFreeLookMode()
        {
            if (!inputEnabled) return;

            if (!isFreeLookMode) return;

            isFreeLookMode = false;
            if (cameraGimbalController != null)
            {
                cameraGimbalController.ResetGimbal(true);
            }

            if (GameStateManager.Instance != null && freeLookExitGameState != null)
            {
                GameStateManager.Instance.EnterGameState(freeLookExitGameState);
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


        protected override void InputUpdate()
        {
            FreeLookModeUpdate();
        }
    }
}