using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Emit particles from a particle system for a specified time.
    /// </summary>
    public class ParticleSystemTimedEmission : MonoBehaviour
    {
        [Tooltip("The particle system to emit from.")]
        [SerializeField]
        protected ParticleSystem m_ParticleSystem;

        [Tooltip("How long to emit for.")]
        [SerializeField]
        protected float emitTime = 2;

        [Tooltip("Whether to emit when the object becomes active in the scene.")]
        [SerializeField]
        protected bool emitOnEnable = true;

        protected ParticleSystem.EmissionModule emissionModule;
        protected ParticleSystemRenderer m_Renderer;
        protected ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule;

        protected float startTime;
        

        protected virtual void Awake()
        {
            emissionModule = m_ParticleSystem.emission;
            colorOverLifetimeModule = m_ParticleSystem.colorOverLifetime;
            m_Renderer = m_ParticleSystem.GetComponent<ParticleSystemRenderer>();
        }


        protected virtual void OnEnable()
        {
            if (emitOnEnable) Emit();
        }


        /// <summary>
        /// Begin emitting particles.
        /// </summary>
        public virtual void Emit()
        {
            startTime = Time.time;
            emissionModule.enabled = true;
        }


        protected virtual void Update()
        {
            if (emissionModule.enabled && Time.time - startTime > emitTime)
            {
                emissionModule.enabled = false;
            }
        }
    }
}
