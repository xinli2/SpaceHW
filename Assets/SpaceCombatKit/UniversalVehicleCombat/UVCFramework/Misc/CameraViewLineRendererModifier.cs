using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;

namespace VSX.CameraSystem
{
    /// <summary>
    /// Controls the appearance of a line renderer based on the camera view. Can be used to adjust weapon visuals in first person vs third person view.
    /// </summary>
    public class CameraViewLineRendererModifier : MonoBehaviour, ICameraEntityUser
    {
        protected CameraEntity cameraEntity;

        [Tooltip("The line renderer to control.")]
        [SerializeField]
        protected LineRenderer m_LineRenderer;

        protected float defaultStartWidth;  // the start width of the line renderer by default
        protected float defaultEndWidth;    // the end width of the line renderer by default

        [Tooltip("The line renderer settings for different camera views.")]
        [SerializeField]
        protected List<CameraViewLineRendererSettings> settingsList = new List<CameraViewLineRendererSettings>();

        /// <summary>
        /// A container for line renderer settings in a camera view.
        /// </summary>
        [System.Serializable]
        public class CameraViewLineRendererSettings
        {
            public CameraView cameraView;
            public float widthMultiplier = 1;
        }


        protected virtual void Reset()
        {
            m_LineRenderer = GetComponentInChildren<LineRenderer>();
        }


        protected virtual void Awake()
        {
            defaultStartWidth = m_LineRenderer.startWidth;
            defaultEndWidth = m_LineRenderer.endWidth;
        }


        /// <summary>
        /// Called when a camera entity follows the camera target that the line renderer is part of.
        /// </summary>
        /// <param name="cameraEntity">The camera entity following this target.</param>
        public void SetCameraEntity(CameraEntity cameraEntity)
        {
            if (cameraEntity != null)
            {
                cameraEntity.onCameraViewTargetChanged.RemoveListener(OnCameraViewTargetChanged);
            }

            m_LineRenderer.startWidth = defaultStartWidth;
            m_LineRenderer.endWidth = defaultEndWidth;

            this.cameraEntity = cameraEntity;

            if (cameraEntity != null)
            {
                cameraEntity.onCameraViewTargetChanged.AddListener(OnCameraViewTargetChanged);
                OnCameraViewTargetChanged(cameraEntity.CurrentViewTarget);
            }
        }


        /// <summary>
        /// Called when the camera view target currently being followed by the camera changes.
        /// </summary>
        /// <param name="cameraViewTarget">The new camera view target.</param>
        protected virtual void OnCameraViewTargetChanged(CameraViewTarget cameraViewTarget)
        {
            m_LineRenderer.startWidth = defaultStartWidth;
            m_LineRenderer.endWidth = defaultEndWidth;

            if (cameraViewTarget == null) return;

            foreach (CameraViewLineRendererSettings settings in settingsList)
            {
                if (settings.cameraView == cameraViewTarget.CameraView)
                {
                    m_LineRenderer.startWidth = defaultStartWidth * settings.widthMultiplier;
                    m_LineRenderer.endWidth = defaultEndWidth * settings.widthMultiplier;
                }
            }
        }
    }
}

