using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class RigidbodyCharacterFootstepAudio : MonoBehaviour
    {

        [SerializeField]
        protected RigidbodyCharacterController m_RigidbodyCharacterController;

        [SerializeField]
        protected RigidbodyCharacterFootstepController footstepController;

        [SerializeField]
        protected AudioSource footstepAudio;

        public ParticleSystem leftFootParticleEffect;
        public ParticleSystem rightFootParticleEffect;

        public int particleEmissionCount = 15;

        [SerializeField]
        protected AudioSource landedAudio;

        [SerializeField]
        protected float maxLandedAudioVolume = 1;


        protected void Reset()
        {
            m_RigidbodyCharacterController = GetComponent<RigidbodyCharacterController>();
        }

        protected virtual void Awake()
        {
            m_RigidbodyCharacterController.onGrounded.AddListener(OnGrounded);

            if (footstepController != null)
            {
                footstepController.onLeftFootDown.AddListener(OnLeftFootstep);
                footstepController.onRightFootDown.AddListener(OnRightFootstep);
            }
        }

        protected virtual void OnLeftFootstep()
        {
            if (leftFootParticleEffect != null) leftFootParticleEffect.Emit(particleEmissionCount);
            OnFootstep();
        }

        protected virtual void OnRightFootstep()
        {
            if (rightFootParticleEffect != null) rightFootParticleEffect.Emit(particleEmissionCount);
            OnFootstep();
        }

        protected virtual void OnFootstep()
        {

            float walkAmount = Mathf.Min((m_RigidbodyCharacterController.Rigidbody.velocity.magnitude / m_RigidbodyCharacterController.WalkSpeed), 1);

            float runMargin = m_RigidbodyCharacterController.RunSpeed - m_RigidbodyCharacterController.WalkSpeed;
            float runAmount = (runMargin == 0 || m_RigidbodyCharacterController.Reversing) ? 0 : Mathf.Max(m_RigidbodyCharacterController.Rigidbody.velocity.magnitude - m_RigidbodyCharacterController.WalkSpeed, 0) / runMargin;

            float movement = walkAmount * 0.5f + runAmount * 0.5f;

            footstepAudio.volume = Mathf.Abs(movement) * 0.3f;
            footstepAudio.Play();
        }

        protected virtual void OnGrounded(Vector3 velocity)
        {
            float fallSpeed = Vector3.Dot(velocity, -Vector3.up);

            float amount = Mathf.Min(fallSpeed / m_RigidbodyCharacterController.MaxFallSpeed, 1);
            landedAudio.volume = amount * maxLandedAudioVolume;
            landedAudio.Play();

        }
    }
}
