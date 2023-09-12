using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Controls a rotary weapon such as a gatling gun.
    /// </summary>
    public class RotatingWeapon : MonoBehaviour
    {

        [Tooltip("The transform that rotates as the weapon fires.")]
        [SerializeField] protected Transform rotatingTransform;

        [Tooltip("The rotation speed of the weapon in degrees/sec.")]
        [SerializeField] protected float rotationSpeed = -700;

        [Tooltip("The time taken for the weapon to accelerate to full rotation speed.")]
        [SerializeField] protected float accelerationTime = 1;
        public float AccelerationTime
        {
            get { return accelerationTime; }
            set { accelerationTime = value; }
        }

        [Tooltip("The time taken for the weapon to decelerate to zero when stopped firing.")]
        [SerializeField] protected float decelerationTime = 1;
        public float DecelerationTime
        {
            get { return decelerationTime; }
            set { decelerationTime = value; }
        }

        [Tooltip("The Triggerable component controlling the weapon firing.")]
        [SerializeField] protected Triggerable triggerable;

        [Tooltip("The weapon rotation audio.")]
        [SerializeField] protected AudioSource rotationAudio;

        [Tooltip("The maximum rotation audio volume.")]
        [SerializeField] protected float maxRotationAudioVolume = 0.33f;

        [Tooltip("The zero rotation speed audio pitch.")]
        [SerializeField] protected float startRotationAudioPitch = 1.33f;

        [Tooltip("The full rotation speed audio pitch.")]
        [SerializeField] protected float fullSpeedRotationAudioPitch = 2.2f;

        protected bool rotating = false;
        protected float stateChangeTime = -1000;

        protected bool canFire = false;
        public bool CanFire { get { return canFire; } }


        protected virtual void Reset()
        {
            rotatingTransform = transform;
        }


        protected virtual void Awake()
        {
            if (triggerable != null)
            {
                triggerable.onStartTriggering.AddListener(StartRotating);
                triggerable.onStopTriggering.AddListener(StopRotating);
            }

            if (rotationAudio != null) rotationAudio.volume = 0;
        }


        /// <summary>
        /// Start rotating.
        /// </summary>
        public virtual void StartRotating()
        {
            rotating = true;
            stateChangeTime = Time.time;
        }


        /// <summary>
        /// Stop rotating
        /// </summary>
        public virtual void StopRotating()
        {
            rotating = false;
            stateChangeTime = Time.time;
        }


        protected virtual void Update()
        {
            if (rotating)
            {
                float accelerationMultiplier = Mathf.Min((Time.time - stateChangeTime) / accelerationTime, 1);
                rotatingTransform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime * accelerationMultiplier));

                if (rotationAudio != null)
                {
                    rotationAudio.pitch = accelerationMultiplier * fullSpeedRotationAudioPitch + (1 - accelerationMultiplier) * startRotationAudioPitch;
                    rotationAudio.volume = maxRotationAudioVolume * accelerationMultiplier;
                }
            }
            else
            {
                float decelerationMultiplier = (Time.time - stateChangeTime) / decelerationTime;
                if (decelerationMultiplier < 1)
                {
                    rotatingTransform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime * (1 - decelerationMultiplier)));

                    if (rotationAudio != null)
                    {
                        rotationAudio.pitch = decelerationMultiplier * startRotationAudioPitch + (1 - decelerationMultiplier) * fullSpeedRotationAudioPitch;
                        rotationAudio.volume = maxRotationAudioVolume * (1 - decelerationMultiplier);
                    }
                }
                else
                {
                    if (rotationAudio != null)
                    {
                        rotationAudio.volume = 0;
                    }
                }
            }

            canFire = rotating && (Time.time - stateChangeTime > accelerationTime);

        }
    }
}
