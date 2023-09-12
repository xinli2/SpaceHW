using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    
	/// <summary>
    /// Controls the hit effect that is shown when a beam strikes a surface.
    /// </summary>
	public class BeamHitEffectController : MonoBehaviour 
	{

        [Header("Events")]

        public UnityEvent onActivated;
        public UnityEvent onDeactivated;

        public UnityEvent onHit;

        [Tooltip("Whether to toggle the particle system on and off based on whether the beam level is approximately 0 or not.")]
        [SerializeField] protected bool toggleParticleSystemEmission = true;

        [Tooltip("Whether to unparent this on awake, necessary for world space particle systems attached to rigged meshes to prevent glitches.")]
        [SerializeField] protected bool unparentOnAwake = false;

        protected ParticleSystem[] particleSystems;
        protected ParticleSystem.EmissionModule[] particleSystemEmissionModules;


        public List<ModulatedAudio> hitEffectAudios = new List<ModulatedAudio>();

        public List<LightAnimator> hitEffectLights = new List<LightAnimator>();


        protected virtual void Awake()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
            particleSystemEmissionModules = new ParticleSystem.EmissionModule[particleSystems.Length];
            for(int i = 0; i < particleSystems.Length; ++i)
            {
                particleSystemEmissionModules[i] = particleSystems[i].emission;
            }

            if (unparentOnAwake) transform.SetParent(null);

            SetLevel(0);
        }

        /// <summary>
        /// Set the 'on' level of the hit effect.
        /// </summary>
        /// <param name="level">The 'on' level.</param>
        public virtual void SetLevel(float level) 
        {
            if (toggleParticleSystemEmission)
            {
                for (int i = 0; i < particleSystems.Length; ++i)
                {
                    particleSystemEmissionModules[i].enabled = !Mathf.Approximately(level, 0);
                }
            }

            for(int i = 0; i < hitEffectAudios.Count; ++i)
            {
                hitEffectAudios[i].SetLevel(level);
            }

            for(int i = 0; i < hitEffectLights.Count; ++i)
            {
                hitEffectLights[i].IntensityMultiplier = level;
            }
        }

        /// <summary>
        /// Set the activation of the hit effect.
        /// </summary>
        /// <param name="activate">Whether it is activated or not.</param>
        public virtual void SetActivation(bool activate)
        {
            gameObject.SetActive(activate);

            // Call the right event
            if (activate)
            {
                onActivated.Invoke();
            }
            else
            {
                onDeactivated.Invoke();
            }
        }

        /// <summary>
        /// Do stuff when the beam hit something.
        /// </summary>
        /// <param name="hit">The hit information.</param>
        public virtual void OnHit(RaycastHit hit)
        {
            gameObject.SetActive(true);
            transform.position = hit.point;
            transform.rotation = Quaternion.LookRotation(hit.normal);

            // Call the event
            onHit.Invoke();
        }
    }
}