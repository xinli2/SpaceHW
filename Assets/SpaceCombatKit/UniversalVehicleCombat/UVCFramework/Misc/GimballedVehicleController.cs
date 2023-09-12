using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Controller for a gimbal feature on a vehicle (a rotating joint such as a turret or mech torso).
    /// </summary>
    public class GimballedVehicleController : MonoBehaviour
    {

        [Header("Settings")]

        [Tooltip("The gimbal/joint to be controlled.")]
        [SerializeField]
        protected GimbalController gimbalController;
        public GimbalController GimbalController { get { return gimbalController; } }

        [Tooltip("Whether the gimbal rotation will be independent of any parent transform rotations.")]
        [SerializeField]
        protected bool independentRotationEnabled = true;
        public bool IndependentRotationEnabled
        {
            get { return independentRotationEnabled; }
            set { independentRotationEnabled = value; }
        }

        // Keep track of horizontal rotation to enable independent rotation.
        protected Quaternion independentHorizontalGimbalRotation;

        protected float horizontalRotationInput;
        protected float verticalRotationInput;

        protected bool rotationInputEnabled = true;

        [Tooltip("The rotation speed of the gimbal.")]
        [SerializeField]
        protected float rotationSpeed = 60;
        public float RotationSpeed
        {
            get { return rotationSpeed; }
            set { rotationSpeed = value; }
        }

        [Tooltip("A list of transforms that will follow the rotation of other transforms after the gimbal is updated.")]
        [SerializeField]
        protected List<TransformFollower> followers = new List<TransformFollower>();

        [Header("Audio")]

        [SerializeField]
        protected AudioSource rotationAudio;

        [SerializeField]
        protected float fullRotationAudioVolume = 1;

        [SerializeField]
        protected AnimationCurve rotationAudioVolumeCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        protected float zeroRotationAudioPitch = 1;

        [SerializeField]
        protected float fullRotationAudioPitch = 1;

        [SerializeField]
        protected AnimationCurve rotationAudioPitchCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Events")]

        public UnityEvent onLateUpdateComplete; // Called when this component has completely finished updating in Late Update.

        public UnityEvent onGimbalControllerUpdated; // Called straight after the gimbal controller is rotated.

        Coroutine fixedUpdateCoroutine;


        protected virtual void Reset()
        {
            gimbalController = transform.GetComponentInChildren<GimbalController>();
        }


        protected virtual void OnEnable()
        {
            if (rotationAudio != null)
            {
                rotationAudio.volume = 0;
                rotationAudio.loop = true;
            }

            fixedUpdateCoroutine = StartCoroutine(AfterFixedUpdate());
        }


        protected virtual void OnDisable()
        {
            StopCoroutine(fixedUpdateCoroutine);
        }

        public virtual void LookAt(Vector3 position, bool snapToTarget = false)
        {
            float angle;
            gimbalController.TrackPosition(position, out angle, snapToTarget);
            UpdateIndependentGimbalRotation();
            UpdateTransformFollowers();
        }

        public void SetRotationInputEnabled(bool setEnabled)
        {
            rotationInputEnabled = setEnabled;
        }

        /// <summary>
        /// Set the rotation inputs from an input script.
        /// </summary>
        /// <param name="horizontalInput">The horizontal input value.</param>
        /// <param name="verticalInput">The vertical input value.</param>
        public virtual void SetRotationInputs(float horizontalInput, float verticalInput)
        {
            horizontalRotationInput = horizontalInput;
            verticalRotationInput = verticalInput;
        }


        protected virtual void LateUpdate()
        {
            // If independent rotation is enabled, use the last recorded rotation to remove any rotations added by parent transforms.
            if (independentRotationEnabled)
            {
                ImplementIndependentGimbalRotation();
            }

            UpdateTransformFollowers();

            onLateUpdateComplete.Invoke();
        }


        // Update the 'follower' transforms to match their 'target' transforms.
        protected virtual void UpdateTransformFollowers()
        {
            Vector3 followerForward;
            for (int i = 0; i < followers.Count; ++i)
            {
                switch (followers[i].rotationFollowType)
                {
                    case TransformRotationFollowType.FullRotation:

                        followers[i].follower.rotation = followers[i].target.rotation * Quaternion.Euler(followers[i].eulerRotationOffset);
                        break;

                    case TransformRotationFollowType.HorizontalOnly:

                        followerForward = followers[i].target.forward;
                        followerForward.y = 0;
                        followerForward.Normalize();
                        followers[i].follower.rotation = Quaternion.LookRotation(followerForward, followers[i].follower.up);
                        break;

                    case TransformRotationFollowType.LookDirectionOnly:

                        followerForward = followers[i].target.forward;
                        followers[i].follower.rotation = Quaternion.LookRotation(followerForward, followers[i].follower.up);
                        break;

                }
            }
        }


        // Reset the gimbal rotation to the last recorded to remove rotations not implemented through this component.
        protected virtual void ImplementIndependentGimbalRotation()
        {
            gimbalController.HorizontalPivot.rotation = independentHorizontalGimbalRotation;
            onGimbalControllerUpdated.Invoke();
        }


        // Update the independent rotation value
        public virtual void UpdateIndependentGimbalRotation()
        {
            independentHorizontalGimbalRotation = gimbalController.HorizontalPivot.rotation;
        }


        /// <summary>
        /// Rotate the gimbal to look at a specified world position.
        /// </summary>
        /// <param name="lookPosition">The world position to look at.</param>
        public virtual void TrackPosition(Vector3 targetPosition)
        {
            float angle;
            gimbalController.TrackPosition(targetPosition, out angle, false);
            if (independentRotationEnabled) UpdateIndependentGimbalRotation();
        }


        protected virtual void OnAnimatorMove()
        {
            if (independentRotationEnabled) UpdateIndependentGimbalRotation();
        }


        // Called after fixed update is complete on all scripts.
        protected IEnumerator AfterFixedUpdate()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();

                // Update the independent rotation
                if (independentRotationEnabled)
                {
                    ImplementIndependentGimbalRotation();
                }
                else
                {
                    onGimbalControllerUpdated.Invoke();
                }

                UpdateTransformFollowers();
            }
        }


        // Physics update
        protected virtual void FixedUpdate()
        {

            // Implement independent rotation
            if (independentRotationEnabled)
            {
                ImplementIndependentGimbalRotation();
            }

            if (rotationInputEnabled)
            {
                // Rotate the gimbal
                gimbalController.Rotate(horizontalRotationInput * Time.fixedDeltaTime * rotationSpeed, verticalRotationInput * Time.fixedDeltaTime * rotationSpeed);

                if (rotationAudio != null)
                {
                    float rotationVal = Mathf.Max(Mathf.Abs(horizontalRotationInput), Mathf.Abs(verticalRotationInput));
                    rotationAudio.volume = rotationAudioVolumeCurve.Evaluate(rotationVal) * fullRotationAudioVolume;

                    float pitchVal = rotationAudioPitchCurve.Evaluate(rotationVal);
                    rotationAudio.pitch = pitchVal * fullRotationAudioPitch + (1 - pitchVal) * zeroRotationAudioPitch;
                }
            }

            // Call the event
            onGimbalControllerUpdated.Invoke();

            // Update the independent rotation value
            if (independentRotationEnabled)
            {
                UpdateIndependentGimbalRotation();
            }
        }
    }
}
