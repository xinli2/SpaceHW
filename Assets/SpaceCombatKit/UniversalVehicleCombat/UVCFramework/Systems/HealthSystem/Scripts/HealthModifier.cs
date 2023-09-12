using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Holds information about damage or healing properties for something that can apply healing or damage, such as a weapon.
    /// </summary>
    [System.Serializable]
    public class HealthModifier
    {
        [Tooltip("The type of health modifier (used to drive different audio/visual effects etc).")]
        [SerializeField]
        protected HealthModifierType healthModifierType;
        public HealthModifierType HealthModifierType
        {
            get { return healthModifierType; }
        }


        [Header("Damage")]


        [Tooltip("The default damage value that this health modifier applies for all health types.")]
        [SerializeField]
        protected float defaultDamageValue = 100;
        public float DefaultDamageValue
        {
            get { return defaultDamageValue; }
            set { defaultDamageValue = value; }
        }


        [Tooltip("Specific damage values to apply for different health types.")]
        [SerializeField]
        protected List<HealthModifierValue> damageOverrideValues = new List<HealthModifierValue>();
        public List<HealthModifierValue> DamageOverrideValues
        {
            get { return damageOverrideValues; }
            set { damageOverrideValues = value; }
        }


        [Tooltip("The dynamic damage effect multiplier.")]
        [SerializeField]
        protected float damageMultiplier = 1;
        public float DamageMultiplier
        {
            get { return damageMultiplier; }
            set { damageMultiplier = value; }
        }


        [Header("Healing")]


        [Tooltip("The default healing value that this health modifier applies for all health types.")]
        [SerializeField]
        protected float defaultHealingValue = 0;
        public float DefaultHealingValue
        {
            get { return defaultHealingValue; }
            set { defaultHealingValue = value; }
        }


        [Tooltip("Specific healing values to apply for different health types.")]
        [SerializeField]
        protected List<HealthModifierValue> healingOverrideValues = new List<HealthModifierValue>();
        public List<HealthModifierValue> HealingOverrideValues
        {
            get { return healingOverrideValues; }
            set { healingOverrideValues = value; }
        }


        [Tooltip("The dynamic healing effect multiplier.")]
        [SerializeField]
        protected float healingMultiplier = 1;
        public float HealingMultiplier
        {
            get { return healingMultiplier; }
            set { healingMultiplier = value; }
        }


        /// <summary>
        /// Get the amount of damage to apply for a specific health type.
        /// </summary>
        /// <param name="healthType">The health type to get damage info for.</param>
        /// <returns>The damage to apply.</returns>
        public virtual float GetDamage(HealthType healthType)
        {
            for (int i = 0; i < damageOverrideValues.Count; ++i)
            {
                if (damageOverrideValues[i].HealthType == healthType)
                {
                    return damageOverrideValues[i].Value;
                }
            }

            return defaultDamageValue;
        }


        /// <summary>
        /// Get the amount of healing to apply for a specific health type.
        /// </summary>
        /// <param name="healthType">The health type to get healing info for.</param>
        /// <returns>The healing to apply.</returns>
        public virtual float GetHealing(HealthType healthType)
        {
            for (int i = 0; i < healingOverrideValues.Count; ++i)
            {
                if (healingOverrideValues[i].HealthType == healthType)
                {
                    return healingOverrideValues[i].Value;
                }
            }

            return defaultHealingValue;
        }
    }
}