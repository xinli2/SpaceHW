using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat.Space
{

    /// <summary>
    /// Controls the sound effects for a space vehicle engine.
    /// </summary>
    public class EngineAudioController : MonoBehaviour
    {

        [Tooltip("The engines component that this component controls sound effects for.")]
        [SerializeField]
        protected VehicleEngines3D engines;

        [Tooltip("The audio source.")]
        [SerializeField]
        protected AudioSource m_Audio;

        [Tooltip("How fast the audio changes in response to changes in the engine control.")]
        [SerializeField]
        protected float changeSpeed = 3;
        float currentLevel = 0;

        public enum EngineAudioControlType
        {
            Movement,
            Boost,
            Steering
        }

        [Tooltip("The type of engine control that the audio is for.")]
        [SerializeField]
        protected EngineAudioControlType controlType;

        public enum AxisContributionType
        {
            Maximum,
            Cumulative
        }

        [Tooltip("How the three axes (X, Y, Z) contribute together to make up the audio effect. ")]
        [SerializeField]
        protected AxisContributionType axisContribution;

        [Tooltip("How much the X axis contributes (for movement/boost, this is left/right, for steering this is nose up/down).")]
        [SerializeField]
        protected float xAxisContribution = 1;

        [Tooltip("How much the Y axis contributes (for movement/boost, this is up/down, for steering this is nose left/right).")]
        [SerializeField]
        protected float yAxisContribution = 1;

        [Tooltip("How much the Z axis contributes (for movement/boost, this is forward/back, for steering this is roll).")]
        [SerializeField]
        protected float zAxisContribution = 1;


        [Header("Volume")]

        [Tooltip("The minimum volume (when the engine control is off).")]
        [SerializeField]
        protected float minVolume = 0;

        [Tooltip("The maximum volume (when the engine control is fully on).")]
        [SerializeField]
        protected float maxVolume = 1;

        [Tooltip("The curve describing how the volume goes from minimum to maximum as the engine control value goes from 0 to 1.")]
        [SerializeField]
        protected AnimationCurve volumeCurve = AnimationCurve.Linear(0, 0, 1, 1);


        [Header("Pitch")]

        [Tooltip("The minimum pitch (when the engine control is off).")]
        [SerializeField]
        protected float minPitch = 0;

        [Tooltip("The maximum pitch (when the engine ccontrol is fully on).")]
        [SerializeField]
        protected float maxPitch = 1;

        [Tooltip("The curve describing how the pitch goes from minimum to maximum as the engine control value goes from 0 to 1.")]
        [SerializeField]
        protected AnimationCurve pitchCurve = AnimationCurve.Linear(0, 1, 1, 1);


        protected virtual void Reset()
        {
            engines = transform.root.GetComponentInChildren<VehicleEngines3D>();
            m_Audio = GetComponentInChildren<AudioSource>();
        }


        protected virtual void Awake()
        {
            if (engines != null)
            {
                engines.onEnginesActivated.AddListener(OnEnginesActivated);
                engines.onEnginesDeactivated.AddListener(OnEnginesDeactivated);
            }

            currentLevel = CalculateLevel();
        }


        protected virtual void OnEnginesActivated()
        {
            m_Audio.volume = 0;
            m_Audio.loop = true;
            m_Audio.Play();
        }


        protected virtual void OnEnginesDeactivated()
        {
            m_Audio.Stop();
        }


        protected virtual void SetAudioLevel(float level)
        {
            float volumeAmount = volumeCurve.Evaluate(level);
            m_Audio.volume = volumeAmount * maxVolume + (1 - volumeAmount) * minVolume;

            float pitchAmount = pitchCurve.Evaluate(level);
            m_Audio.pitch = pitchAmount * maxPitch + (1 - pitchAmount) * minPitch;
        }


        protected virtual float CalculateLevel()
        {
            float level = 0;

            // Volume
            switch (axisContribution)
            {
                case AxisContributionType.Maximum:

                    switch (controlType)
                    {
                        case EngineAudioControlType.Movement:

                            level = Mathf.Max(level, Mathf.Abs(engines.MovementInputs.x) * xAxisContribution);
                            level = Mathf.Max(level, Mathf.Abs(engines.MovementInputs.y) * yAxisContribution);
                            level = Mathf.Max(level, Mathf.Abs(engines.MovementInputs.z) * zAxisContribution);

                            break;

                        case EngineAudioControlType.Boost:

                            level = Mathf.Max(level, Mathf.Abs(engines.BoostInputs.x) * xAxisContribution);
                            level = Mathf.Max(level, Mathf.Abs(engines.BoostInputs.y) * yAxisContribution);
                            level = Mathf.Max(level, Mathf.Abs(engines.BoostInputs.z) * zAxisContribution);

                            break;

                        case EngineAudioControlType.Steering:

                            level = Mathf.Max(level, Mathf.Abs(engines.SteeringInputs.x) * xAxisContribution);
                            level = Mathf.Max(level, Mathf.Abs(engines.SteeringInputs.y) * yAxisContribution);
                            level = Mathf.Max(level, Mathf.Abs(engines.SteeringInputs.z) * zAxisContribution);

                            break;
                    }


                    break;

                default:    // Cumulative

                    switch (controlType)
                    {
                        case EngineAudioControlType.Movement:

                            level += (Mathf.Abs(engines.MovementInputs.x) / 3f) * xAxisContribution;
                            level += (Mathf.Abs(engines.MovementInputs.y) / 3f) * yAxisContribution;
                            level += (Mathf.Abs(engines.MovementInputs.z) / 3f) * zAxisContribution;

                            break;

                        case EngineAudioControlType.Boost:

                            level += (Mathf.Abs(engines.BoostInputs.x) / 3f) * xAxisContribution;
                            level += (Mathf.Abs(engines.BoostInputs.y) / 3f) * yAxisContribution;
                            level += (Mathf.Abs(engines.BoostInputs.z) / 3f) * zAxisContribution;

                            break;

                        case EngineAudioControlType.Steering:

                            level += (Mathf.Abs(engines.SteeringInputs.x) / 3f) * xAxisContribution;
                            level += (Mathf.Abs(engines.SteeringInputs.y) / 3f) * yAxisContribution;
                            level += (Mathf.Abs(engines.SteeringInputs.z) / 3f) * zAxisContribution;

                            break;
                    }

                    break;
            }

            return level;

        }


        // Called every frame
        protected virtual void Update()
        {
            if (engines != null)
            {
                currentLevel = Mathf.Lerp(currentLevel, CalculateLevel(), changeSpeed * Time.deltaTime);

                SetAudioLevel(currentLevel);
            }
        }
    }
}