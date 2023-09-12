using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.Pooling;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Control sound effects damage and healing of a Damageable component.
    /// </summary>
    public class DamageableAudioController : MonoBehaviour
    {
        [Tooltip("The Damageable component the sound effects are for.")]
        [SerializeField]
        protected Damageable damageable;

        [Tooltip("The default damage sound effect (used when no overrides are found for the specified Health Modifier Type).")]
        [SerializeField]
        protected AudioSource defaultDamageAudio;

        [Tooltip("The damage sound effect overrides for specific Health Modifier Types.")]
        [SerializeField]
        protected List<HealthModifierTypeAudio> healthModifierTypeDamageAudioOverrides = new List<HealthModifierTypeAudio>();

        [Tooltip("The default healing sound effect (used when no overrides are found for the specified Health Modifier Type).")]
        [SerializeField]
        protected AudioSource defaultHealingAudio;

        [Tooltip("The healing sound effect overrides for specific Health Modifier Types.")]
        [SerializeField]
        protected List<HealthModifierTypeAudio> healthModifierTypeHealingAudioOverrides = new List<HealthModifierTypeAudio>();


        /// <summary>
        /// Represents an audio source associated with a Health Modifier Type.
        /// </summary>
        [System.Serializable]
        public class HealthModifierTypeAudio
        {
            public HealthModifierType healthModifierType;
            public AudioSource audioSource;
        }


        // Called when the component is first added to a gameobject or reset in the inspector.
        protected virtual void Reset()
        {
            damageable = transform.GetComponentInChildren<Damageable>();
        }


        protected virtual void Awake()
        {
            if (damageable != null)
            {
                damageable.onDamaged.AddListener(OnDamage);
                damageable.onHealed.AddListener(OnHeal);
            }
        }


        protected virtual void PlayAudio(AudioSource audio, Vector3 position)
        {
            if (audio == null) return;

            if (audio.gameObject.scene == null || audio.gameObject.scene.name == null)
            {
                PoolManager.Instance.Get(audio.gameObject, position, Quaternion.identity);
            }
            else
            {
                audio.Play();
            }
        }


        // Function called when the Damageable is damaged.
        protected virtual void OnDamage(HealthEffectInfo info)
        {
            bool found = false;
            for (int i = 0; i < healthModifierTypeDamageAudioOverrides.Count; ++i)
            {
                if (healthModifierTypeDamageAudioOverrides[i].healthModifierType == info.healthModifierType)
                {
                    PlayAudio(healthModifierTypeDamageAudioOverrides[i].audioSource, info.worldPosition);

                    found = true;
                }
            }

            if (!found)
            {
                PlayAudio(defaultDamageAudio, info.worldPosition);
            }
        }


        // Function called when the Damageable is healed.
        protected virtual void OnHeal(HealthEffectInfo info)
        {

            bool found = false;
            for (int i = 0; i < healthModifierTypeHealingAudioOverrides.Count; ++i)
            {
                if (healthModifierTypeHealingAudioOverrides[i].healthModifierType == info.healthModifierType)
                {
                    PlayAudio(healthModifierTypeHealingAudioOverrides[i].audioSource, info.worldPosition);
                    found = true;
                }
            }

            if (!found)
            {
                PlayAudio(defaultHealingAudio, info.worldPosition);
            }
        }
    }
}

