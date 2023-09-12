using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Provides an example of a visual effects manager for a jet exhaust.
    /// </summary>
    public class JetExhaustVisualEffectsController : MonoBehaviour
    {
        
        [Header("General")]

        [Tooltip("The engines component that drives these visual effects.")]
        [SerializeField]
        protected Engines engines;

        [SerializeField]
        protected float effectMultiplier = 1;
        public float EffectMultiplier
        {
            get { return effectMultiplier; }
            set { effectMultiplier = value; }
        }

        [Header("Visual Elements")]

        // Glow renderers

        [Tooltip("All the exhaust glow renderers.")]
        [SerializeField]
        protected List<MeshRenderer> exhaustGlowRenderers = new List<MeshRenderer>();
        protected List<Material> exhaustGlowMaterials = new List<Material>();

        [Tooltip("The key for accessing the color property of the shader used by the exhaust glow materials.")]
        [SerializeField]
        protected string exhaustGlowShaderColorName = "_Color";

        // Halo renderers
        [Tooltip("All the exhaust halo renderers. May not be necessary if using a bloom effect.")]
        [SerializeField]
        protected List<MeshRenderer> exhaustHaloRenderers = new List<MeshRenderer>();
        protected List<Material> exhaustHaloMaterials = new List<Material>();

        [Tooltip("The key for accessing the color property of the shader used by the exhaust halo materials.")]
        [SerializeField]
        protected string exhaustHaloShaderColorName = "_Color";

        // Particle systems

        [Tooltip("All the exhaust particle systems.")]
        [SerializeField]
        protected List<ParticleSystem> exhaustParticleSystems = new List<ParticleSystem>();
        protected ParticleSystem.MainModule[] exhaustParticleSystemMainModules;
        protected ParticleSystem.EmissionModule[] exhaustParticleSystemEmissionModules;
        protected List<Material> exhaustParticleMaterials = new List<Material>();
        protected List<float> exhaustParticleStartSpeeds = new List<float>();

        [Tooltip("The key for accessing the color property of the shader used by the exhaust particle system materials.")]
        [SerializeField]
        protected string exhaustParticleShaderColorName = "_Color";


        [Header("Trail Renderers")]

        [Tooltip("All the exhaust trail renderers.")]
        [SerializeField]
        protected List<TrailRenderer> exhaustTrailRenderers = new List<TrailRenderer>();
        protected List<Material> exhaustTrailMaterials = new List<Material>();

        [Tooltip("The key for accessing the color property of the shader used by the exhaust trail materials.")]
        [SerializeField]
        protected string exhaustTrailShaderColorName = "_Color";

        [Tooltip("Whether to disable the exhaust trail renderers when the scene starts.")]
        [SerializeField]
        protected bool disableExhaustTrailsOnAwake = false;


        [Header("Cruising")]

        [Tooltip("A curve that describes the effects 'amount' as the throttle values change.")]
        [SerializeField]
        protected AnimationCurve throttleValueToEffectsCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("The color of the exhaust from 0 to 1 throttle.")]
        [SerializeField]
        protected Gradient exhaustColorGradient = new Gradient();

        [Tooltip("The alpha of the exhaust glow during cruising at full throttle.")]
        [SerializeField]
        protected float maxCruisingGlowAlpha = 0.8f;

        [Tooltip("The alpha of the exhaust halo during cruising at full throttle.")]
        [SerializeField]
        protected float maxCruisingHaloAlpha = 0.3f;

        [Tooltip("The alpha of the exhaust particles during cruising at full throttle.")]
        [SerializeField]
        protected float maxCruisingParticleAlpha = 0.2f;

        [Tooltip("The speed multiplier of the exhaust particles applied during cruising at full throttle.")]
        [SerializeField]
        protected float maxCruisingParticleSpeedFactor = 1f;

        [Tooltip("The alpha of the exhaust trails during cruising at full throttle.")]
        [SerializeField]
        protected float maxCruisingTrailAlpha = 0.75f;

        [Tooltip("The color multiplier applied to the effects when cruising at full throttle, may be necessary to achieve sufficient bloom when using image effects.")]
        [SerializeField]
        protected float cruisingColorMultiplier = 3;


        [Header("Boost")]

        [Tooltip("The color of the effects during boost.")]
        [SerializeField]
        protected Color boostColor = Color.white;

        [Tooltip("The alpha of the exhaust glow effects during boost.")]
        [SerializeField]
        protected float boostGlowAlpha = 1f;

        [Tooltip("The alpha of the exhaust halo effects during boost.")]
        [SerializeField]
        protected float boostHaloAlpha = 0.4f;

        [Tooltip("The alpha of the exhaust particle effects during boost.")]
        [SerializeField]
        protected float boostParticleAlpha = 0.3f;

        [Tooltip("The speed multiplier of the exhaust particles applied during boost.")]
        [SerializeField]
        protected float boostParticleSpeedFactor = 2f;

        [Tooltip("The alpha of the exhaust trails during boost.")]
        [SerializeField]
        protected float boostTrailAlpha = 1f;

        [Tooltip("The color multiplier applied to the effects during boost, may be necessary to achieve sufficient bloom when using image effects.")]
        [SerializeField]
        protected float boostColorMultiplier = 3;

        protected float cruisingEffectsAmount = Mathf.Infinity, boostEffectsAmount = Mathf.Infinity;
        protected float targetCruisingEffectsAmount = 0, targetBoostEffectsAmount = 0;


        // Called when component is first added to a gameobject or reset in the inspector
        protected virtual void Reset()
        {
            exhaustColorGradient.colorKeys = new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.5f, 0f, 1f), 0) };
            engines = GetComponent<Engines>();
        }


		protected virtual void Awake()
		{

            // Cache all the glow materials

            foreach(MeshRenderer exhaustGlowRenderer in exhaustGlowRenderers)
			{
                foreach (Material mat in exhaustGlowRenderer.materials)
                {
                    exhaustGlowMaterials.Add(mat);
                }
			}

            // Cache all the halo materials

            foreach (MeshRenderer exhaustHaloRenderer in exhaustHaloRenderers)
            {
				foreach (Material mat in exhaustHaloRenderer.materials)
                {
                    exhaustHaloMaterials.Add(mat);
                }
            }

            // Cache the particle system data

            exhaustParticleSystemMainModules = new ParticleSystem.MainModule[exhaustParticleSystems.Count];
            exhaustParticleSystemEmissionModules = new ParticleSystem.EmissionModule[exhaustParticleSystems.Count];
            for (int i = 0; i < exhaustParticleSystems.Count; ++i)
			{

                // Cache materials

                ParticleSystemRenderer particleSystemRenderer = exhaustParticleSystems[i].GetComponent<ParticleSystemRenderer>();
                foreach (Material mat in particleSystemRenderer.materials)
                {
                    exhaustParticleMaterials.Add(mat);
                }

                // Store the particle speed for dynamic control via throttle (0-1) value

                exhaustParticleSystemMainModules[i] = exhaustParticleSystems[i].main;
                exhaustParticleSystemEmissionModules[i] = exhaustParticleSystems[i].emission;
                exhaustParticleStartSpeeds.Add(exhaustParticleSystemMainModules[i].startSpeed.constant);
			}

            // Cache all the trail renderer materials
            
			foreach (TrailRenderer exhaustTrailRenderer in exhaustTrailRenderers)
			{
                foreach (Material mat in exhaustTrailRenderer.materials)
                {
                    exhaustTrailMaterials.Add(mat);
                }
			}

            if (disableExhaustTrailsOnAwake)
            {
                SetExhaustTrailsEnabled(false);
            }
        }


        /// <summary>
		/// Reset and clear the exhaust effects.
		/// </summary>
		public virtual void ResetExhaust()
		{
			for (int i = 0; i < exhaustTrailRenderers.Count; ++i)
			{
				exhaustTrailRenderers[i].Clear();
			}
		}


        /// <summary>
        /// Enable or disable the trail renderers .
        /// </summary>
        /// <param name="setEnabled">Whether the trail renderers will be enabled or disabled.</param>
        public virtual void SetExhaustTrailsEnabled(bool setEnabled)
		{
			for (int i = 0; i < exhaustTrailRenderers.Count; ++i)
			{
				exhaustTrailRenderers[i].enabled = setEnabled;
			}
		}

        protected virtual void CalculateEffectsParameters()
        {
            if (engines == null) return;

            // If engines assigned, use it to drive the effects
            if (!engines.EnginesActivated)
            {
                targetCruisingEffectsAmount = 0;
                targetBoostEffectsAmount = 0;
            }
            else
            {
                targetCruisingEffectsAmount = effectMultiplier * throttleValueToEffectsCurve.Evaluate(engines.MovementInputs.z);
                targetBoostEffectsAmount = effectMultiplier * engines.BoostInputs.z;
            }
        }


        public virtual void UpdateEffects ()
		{
            
            for(int i = 0; i < exhaustParticleSystemEmissionModules.Length; ++i)
            {
                exhaustParticleSystemEmissionModules[i].enabled = !Mathf.Approximately(effectMultiplier * (cruisingEffectsAmount + boostEffectsAmount), 0);
            }

            for (int i = 0; i < exhaustGlowRenderers.Count; ++i)
            {
                exhaustGlowRenderers[i].enabled = !Mathf.Approximately(effectMultiplier * (cruisingEffectsAmount + boostEffectsAmount), 0);
            }

            for (int i = 0; i < exhaustHaloRenderers.Count; ++i)
            {
                exhaustHaloRenderers[i].enabled = !Mathf.Approximately(effectMultiplier * (cruisingEffectsAmount + boostEffectsAmount), 0);
            }

            if (Mathf.Approximately(cruisingEffectsAmount, targetCruisingEffectsAmount) && Mathf.Approximately(boostEffectsAmount, targetBoostEffectsAmount)) return;

            cruisingEffectsAmount = targetCruisingEffectsAmount;
            boostEffectsAmount = targetBoostEffectsAmount;

            
            Color c = (1 - boostEffectsAmount) * cruisingEffectsAmount * exhaustColorGradient.Evaluate(cruisingEffectsAmount) * cruisingColorMultiplier +
                        boostEffectsAmount * boostColor * boostColorMultiplier;

            
            float particleAlpha = (1 - boostEffectsAmount) * cruisingEffectsAmount * maxCruisingParticleAlpha + 
                                boostEffectsAmount * boostParticleAlpha;

            float particleSpeedFactor = (1 - boostEffectsAmount) * cruisingEffectsAmount * maxCruisingParticleSpeedFactor + 
                                    boostEffectsAmount * boostParticleSpeedFactor;

            float haloAlpha = (1 - boostEffectsAmount) * cruisingEffectsAmount * maxCruisingHaloAlpha +
                            boostEffectsAmount * boostHaloAlpha;

            float glowAlpha = (1 - boostEffectsAmount) * cruisingEffectsAmount * maxCruisingGlowAlpha +
                            boostEffectsAmount * boostGlowAlpha;

            float trailAlpha = (1 - boostEffectsAmount) * cruisingEffectsAmount * maxCruisingTrailAlpha +
                            boostEffectsAmount * boostTrailAlpha;

            // Update halo materials
			for (int i = 0; i < exhaustHaloMaterials.Count; ++i)
			{
				c.a = haloAlpha;
				exhaustHaloMaterials[i].SetColor(exhaustHaloShaderColorName, c);
			}
			
            // Update glow materials
			for (int i = 0; i < exhaustGlowMaterials.Count; ++i)
			{
				c.a = glowAlpha;
				exhaustGlowMaterials[i].SetColor(exhaustGlowShaderColorName, c);
			}
			
            // Update particle effects
			for (int i = 0; i < exhaustParticleMaterials.Count; ++i)
			{
				c.a = particleAlpha;
				exhaustParticleMaterials[i].SetColor(exhaustParticleShaderColorName, c);
			}
	
            // Update particle speed
			for (int i = 0; i < exhaustParticleSystemMainModules.Length; ++i)
			{
                exhaustParticleSystemMainModules[i].startSpeed = particleSpeedFactor * exhaustParticleStartSpeeds[i];
			}
				
            // Update trail renderer materials
			for (int i = 0; i < exhaustTrailMaterials.Count; ++i)
			{
				c.a = trailAlpha;
				exhaustTrailMaterials[i].SetColor(exhaustTrailShaderColorName, c);
			}
        }

        // Called every frame
        protected virtual void LateUpdate()
        {
            CalculateEffectsParameters();
            UpdateEffects();
        }
	}
}
