using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Displays a flame effect for a directional thruster.
    /// </summary>
    public class DirectionalThrusterEffect : DirectionalThrusterEffectBase
    {

        [Tooltip("The frequency flickering in the effect.")]
        [SerializeField]
        protected float flickerFrequency = 30;


        [Header("Colors")]

        [Tooltip("Whether this script will control the color of the renderers (rather than just controlling the alpha).")]
        [SerializeField]
        protected bool colorModificationEnabled = true;

        [Tooltip("The thruster effect renderers.")]
        [SerializeField]
        protected List<AnimatedRenderer> effectRenderers = new List<AnimatedRenderer>();

        [Tooltip("The color of the thruster visual effects as the directional thruster goes from 0 to 1.")]
        [GradientUsage(true)]
        [SerializeField]
        protected Gradient effectColor;


        [Header("Light")]

        [Tooltip("The thruster effect light.")]
        [SerializeField]
        protected Light m_Light;

        [Tooltip("The thruster effect light's intensity value when the thruster is fully on.")]
        [SerializeField]
        protected float lightBaseIntensity = 1;

        [Tooltip("The amount of flickering variation in the light.")]
        [SerializeField]
        protected float lightIntensityVariation = 0.2f;


        [Header("Flame Scaling")]

        [Tooltip("The transform representing the flame.")]
        [SerializeField]
        protected Transform flame;

        [Tooltip("The default scale of the flame.")]
        [SerializeField]
        protected float flameBaseScale = 1;

        [Tooltip("The scale variation of the flame as the noise function varies between 0 and 1.")]
        [SerializeField]
        protected float flameScaleVariation = 0.2f;


        [Header("Audio")]

        [Tooltip("The audio source for the thruster.")]
        [SerializeField]
        protected AudioSource m_Audio;

        [Tooltip("How fast the audio responds as a result of changes in the thruster level.")]
        [SerializeField]
        protected float audioLerpSpeed = 6;
        protected float currentAudioLevel;

        [Tooltip("The minimum volume (when the thruster is off).")]
        [SerializeField]
        protected float minVolume = 0;

        [Tooltip("The maximum volume (when the thruster is fully on).")]
        [SerializeField]
        protected float maxVolume = 1;

        [Tooltip("The curve describing how the volume goes from minimum to maximum as the thruster goes from 0 to 1.")]
        [SerializeField]
        protected AnimationCurve volumeCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("The minimum pitch (when the thruster is off).")]
        [SerializeField]
        protected float minPitch = 0;

        [Tooltip("The maximum pitch (when the thruster is fully on).")]
        [SerializeField]
        protected float maxPitch = 1;

        [Tooltip("The curve describing how the pitch goes from minimum to maximum as the thruster goes from 0 to 1.")]
        [SerializeField]
        protected AnimationCurve pitchCurve = AnimationCurve.Linear(0, 1, 1, 1);



        protected override void Reset()
        {
            base.Reset();

            m_Light = GetComponentInChildren<Light>();

            effectColor = new Gradient();
            GradientColorKey startColorKey = new GradientColorKey(new Color(1, 0.5f, 0), 0);
            GradientColorKey endColorKey = new GradientColorKey(new Color(1, 0.5f, 0), 1);
            GradientAlphaKey startAlphaKey = new GradientAlphaKey(0, 0);
            GradientAlphaKey endAlphaKey = new GradientAlphaKey(1, 1);
            effectColor.SetKeys(new GradientColorKey[] { startColorKey, endColorKey }, new GradientAlphaKey[] { startAlphaKey, endAlphaKey });
        }


        protected virtual void Awake()
        {
            if (engines != null)
            {
                engines.onEnginesActivated.AddListener(OnEnginesActivated);
                engines.onEnginesDeactivated.AddListener(OnEnginesDeactivated);
            }
        }


        protected virtual void OnEnginesActivated()
        {
            if (m_Audio != null)
            {
                m_Audio.volume = 0;
                m_Audio.loop = true;
                m_Audio.Play();
            }
        }


        protected virtual void OnEnginesDeactivated()
        {
            if (m_Audio != null)
            {
                m_Audio.Stop();
            }
        }


        // Called every frame
        protected override void Update()
        {
            base.Update();
            UpdateEffect(Level);
        }


        // Update the effect
        protected virtual void UpdateEffect(float level)
        {

            float flickerAmount = Mathf.PerlinNoise(Time.time * flickerFrequency, 0.5f) - 0.5f; // Goes from -0.5 to +0.5


            // Flame scale

            if (flame != null)
            {
                Vector3 flameScale = flame.localScale;

                float nextScaleVariation = flameScaleVariation * flickerAmount;

                flameScale.z = level * (flameBaseScale + nextScaleVariation);
                flame.localScale = flameScale;
            }


            // Effects color

            Color nextEffectColor = effectColor.Evaluate(level);
            foreach (AnimatedRenderer effectRenderer in effectRenderers)
            {
                if (Mathf.Approximately(level, 0))
                {
                    effectRenderer.renderer.enabled = false;
                }
                else
                {
                    effectRenderer.renderer.enabled = true;

                    Color c;

                    if (colorModificationEnabled)
                    {
                        c = nextEffectColor;
                    }
                    else
                    {
                        c = effectRenderer.renderer.material.GetColor(effectRenderer.colorKey);
                    }

                    c.a = nextEffectColor.a * (effectRenderer.preserveAlpha ? effectRenderer.BaseAlpha : 1);

                    effectRenderer.renderer.material.SetColor(effectRenderer.colorKey, c);
                }
            }


            // Lights

            if (m_Light != null)
            {
                if (colorModificationEnabled)
                {
                    m_Light.color = nextEffectColor;
                }

                m_Light.intensity = nextEffectColor.a * lightBaseIntensity + flickerAmount * lightIntensityVariation;

            }


            // Audio

            if (m_Audio != null)
            {
                currentAudioLevel = Mathf.Lerp(currentAudioLevel, level, audioLerpSpeed * Time.deltaTime);
                float volumeAmount = volumeCurve.Evaluate(currentAudioLevel);
                m_Audio.volume = volumeAmount * maxVolume + (1 - volumeAmount) * minVolume;

                float pitchAmount = pitchCurve.Evaluate(currentAudioLevel);
                m_Audio.pitch = pitchAmount * maxPitch + (1 - pitchAmount) * minPitch;
            }
            
        }
    }
}

