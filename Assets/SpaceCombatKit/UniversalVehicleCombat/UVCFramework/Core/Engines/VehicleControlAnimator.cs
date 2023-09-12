using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Animates the vehicle relative to its own root transform, according to the velocity and angular velocity of the rigidbody, to give an extra feel of control to the player.
    /// </summary>
    public class VehicleControlAnimator : MonoBehaviour, ICameraEntityUser
    {
        [Tooltip("The camera views in which vehicle control animation will occur.")]
        [SerializeField]
        protected List<CameraView> cameraViews = new List<CameraView>();

        [Tooltip("The rigidbody that provides the velocity values for animating the vehicle.")]
        [SerializeField]
        protected Rigidbody m_Rigidbody;

        [Tooltip("How fast the animation occurs as a response to a change in velocity or angular velocity of the rigidbody.")]
        [SerializeField]
        protected float animationLerpSpeed = 0.5f;

        [Tooltip("Whether this component is active or not.")]
        [SerializeField]
        protected bool activated = true;

        [Header("Roll")]

        [Tooltip("How much the vehicle rolls while turning left or right.")]
        [SerializeField]
        protected float sideRotationToRoll = 20;

        [Tooltip("How much the vehicle rolls while strafing left or right.")]
        [SerializeField]
        protected float sideMovementToRoll = -0.15f;

        [Header("Turn Following")]

        [Tooltip("How much the vehicle rotates up or down when pitching (rotating up or down).")]
        [SerializeField]
        protected float verticalTurnFollowing = 8;

        [Tooltip("How much the vehicle rotates left or right when yawing (rotating sideways).")]
        [SerializeField]
        protected float sideTurnFollowing = 5;

        protected CameraEntity cameraEntity;



        protected virtual void Reset()
        {
            m_Rigidbody = transform.root.GetComponentInChildren<Rigidbody>();
        }


        /// <summary>
        /// Set the camera entity that is following the vehicle
        /// </summary>
        /// <param name="entity"></param>
        public void SetCameraEntity(CameraEntity cameraEntity)
        {
            // Disconnect event from previous camera
            if (this.cameraEntity != null)
            {
                this.cameraEntity.onCameraViewTargetChanged.RemoveListener(OnCameraViewChanged);
            }

            // Set new camera
            this.cameraEntity = cameraEntity;

            // Connect to event on new camera
            if (this.cameraEntity != null)
            {
                OnCameraViewChanged(cameraEntity.CurrentViewTarget);
                this.cameraEntity.onCameraViewTargetChanged.AddListener(OnCameraViewChanged);
            }
        }


        // Called when the camera view changes
        protected virtual void OnCameraViewChanged(CameraViewTarget newCameraViewTarget)
        {
            ClearAnimation();

            activated = false;

            // Check camera view
            if (cameraViews.Count > 0)
            {
                if (newCameraViewTarget == null) return;

                if (cameraViews.IndexOf(newCameraViewTarget.CameraView) == -1)
                {
                    return;
                }
            }

            activated = true;
        }


        /// <summary>
        /// Remove all animated rotation.
        /// </summary>
        public virtual void ClearAnimation()
        {
            transform.localRotation = Quaternion.identity;
        }


        protected virtual void FixedUpdate()
        {
            if (activated)
            {
                Vector3 localAngularVelocity = m_Rigidbody.transform.InverseTransformDirection(m_Rigidbody.angularVelocity);

                Vector3 localVelocity = m_Rigidbody.transform.InverseTransformDirection(m_Rigidbody.velocity);

                float targetRoll = sideRotationToRoll * -localAngularVelocity.y + sideMovementToRoll * localVelocity.x;
                float targetPitch = verticalTurnFollowing * localAngularVelocity.x;
                float targetYaw = sideTurnFollowing * localAngularVelocity.y;

                // Yaw to roll
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(targetPitch, targetYaw, targetRoll), animationLerpSpeed);
            }
        }
    }
}
