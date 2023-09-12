using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.CameraSystem
{
    /// <summary>
    /// This class manages a secondary camera, which is any additional camera in your scene that must conform to changes in the settings of the main camera.
    /// For example, a background camera that shows the environment, which must copy the rotation and field of view.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SecondaryCamera : MonoBehaviour
    {
        [Tooltip("Finds the first Camera Entity in the scene, if it hasn't been already assigned in the inspector.")]
        [SerializeField]
        protected bool findFirstCameraEntityInScene = true;


        [Tooltip("The camera that this camera follows.")]
        [SerializeField]
        protected CameraEntity cameraEntity;
        public CameraEntity CameraEntity
        {
            set
            {
                // Disconnect events from previous camera entity
                if (cameraEntity != null)
                {
                    cameraEntity.onCameraViewTargetChanged.RemoveListener(OnCameraViewTargetChanged);
                }

                cameraEntity = value;

                // Connect events to new camera entity
                if (cameraEntity != null)
                {
                    cameraEntity.onCameraViewTargetChanged.AddListener(OnCameraViewTargetChanged);
                }
            }
        }


        [Tooltip("Whether to copy the field of view of the camera entity by default.")]
        [SerializeField]
        protected bool copyFieldOfView = true;


        [Tooltip("Whether to copy the rotation of the camera entity by default.")]
        [SerializeField]
        protected bool copyRotation = true;


        [Tooltip("Optional overrides for specified Camera Views.")]
        [SerializeField]
        protected List<SecondaryCameraViewSettings> viewOverrideSettings = new List<SecondaryCameraViewSettings>();

        protected Camera m_Camera;


        protected virtual void Awake()
        {
            m_Camera = GetComponent<Camera>();

            // Connect to first camera entity in scene if it's not set already
            if (cameraEntity == null && findFirstCameraEntityInScene)
            {
                cameraEntity = FindObjectOfType<CameraEntity>();
            }

            CameraEntity = cameraEntity;    // Connect the camera
        }


        /// <summary>
        /// Called when the camera view target changes on the camera.
        /// </summary>
        /// <param name="newCameraViewTarget">The new camera view target.</param>
        public virtual void OnCameraViewTargetChanged(CameraViewTarget cameraViewTarget) { }


        /// <summary>
        /// Update the secondary camera.
        /// </summary>
        public virtual void UpdateSettings()
        {
            // Check for settings for the specific view
            for (int i = 0; i < viewOverrideSettings.Count; ++i)
            {
                if (viewOverrideSettings[i].view == cameraEntity.CurrentView)
                {
                    if (viewOverrideSettings[i].copyFieldOfView) m_Camera.fieldOfView = cameraEntity.MainCamera.fieldOfView;
                    if (viewOverrideSettings[i].copyRotation) transform.rotation = cameraEntity.MainCamera.transform.rotation;
                    return;
                }
            }

            // If no view-specific settings found, use the default settings
            if (copyFieldOfView) m_Camera.fieldOfView = cameraEntity.MainCamera.fieldOfView;
            if (copyRotation) transform.rotation = cameraEntity.MainCamera.transform.rotation;
        }


        protected virtual void LateUpdate()
        {
            if (cameraEntity != null)
            {
                UpdateSettings();
            }
        }
    }
}