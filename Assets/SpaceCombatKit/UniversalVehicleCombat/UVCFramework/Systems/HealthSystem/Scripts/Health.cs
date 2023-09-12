using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Unity event for running functions when a collision occurs
    /// </summary>
    [System.Serializable]
    public class OnCollisionEnterEventHandler : UnityEvent<Collision> { }

    /// <summary>
    /// This class provides a vehicle with a Health component.
    /// </summary>
    public class Health : ModuleManager
    {
        protected Vehicle vehicle;

        // All the Damageables loaded onto this vehicle
        protected List<Damageable> damageables = new List<Damageable>();
        public List<Damageable> Damageables { get { return damageables; } }

        [SerializeField]
        protected bool overrideDamageableDefaultSettings = false;
        public bool OverrideDamageableDefaultSettings { set { overrideDamageableDefaultSettings = value; } }

        [SerializeField]
        protected bool isDamageable = true;
        public bool IsDamageable
        {
            get { return isDamageable; }
            set 
            { 
                isDamageable = value;

                SetDamageableAll(isDamageable);
            }
        }

        [SerializeField]
        protected bool isHealable = true;
        public bool IsHealable
        {
            get { return isHealable; }
            set
            {
                isHealable = value;

                SetHealableAll(isHealable);
            }
        }

      
        [Header("Events")]

        // Collision event
        public OnCollisionEnterEventHandler onCollisionEnter;

   
        protected override void Awake()
        {
            base.Awake();

            vehicle = GetComponent<Vehicle>();
            if (vehicle != null)
            {
                vehicle.onRestored.AddListener(ResetHealth);
            }
            
            DamageReceiver[] damageReceivers = transform.GetComponentsInChildren<DamageReceiver>();
            foreach(DamageReceiver damageReceiver in damageReceivers)
            {
                onCollisionEnter.AddListener(damageReceiver.OnCollision);
            }

            Damageable[] foundDamageables = transform.GetComponentsInChildren<Damageable>();
            foreach (Damageable damageable in foundDamageables)
            {
                AddDamageable(damageable);
            }
        }


        public virtual void AddDamageable(Damageable damageable)
        {
            if (damageables.IndexOf(damageable) == -1)
            {
                damageables.Add(damageable);

                if (overrideDamageableDefaultSettings)
                {
                    damageable.SetDamageable(isDamageable);
                    damageable.SetHealable(isHealable);
                }
            }
        }


        public virtual void RemoveDamageable(Damageable damageable)
        {
            damageables.Remove(damageable);
        }


        /// <summary>
        /// Set the damageability of all damageable components on the vehicle.
        /// </summary>
        /// <param name="isDamageable">Whether the damageables are damageable.</param>
        protected virtual void SetDamageableAll(bool isDamageable)
        {
            for(int i = 0; i < damageables.Count; ++i)
            {
                damageables[i].SetDamageable(isDamageable);
            }
        }

        /// <summary>
        /// Set the healability of all damageable components on the vehicle.
        /// </summary>
        /// <param name="isHealable">Whether the damageables are healable.</param>
        protected virtual void SetHealableAll(bool isHealable)
        {
            for (int i = 0; i < damageables.Count; ++i)
            {
                damageables[i].SetHealable(isHealable);
            }
        }

        public virtual void DestroyAllDamageables()
        {
            for (int i = 0; i < damageables.Count; ++i)
            {
                if (!damageables[i].Destroyed)
                {
                    damageables[i].Destroy();
                }
            }
        }

        // Called when a collision occurs
        protected virtual void OnCollisionEnter(Collision collision)
        {
            // Call the collision event
            onCollisionEnter.Invoke(collision);
        }


        /// <summary>
        /// Called every time a new module is mounted at a module mount.
        /// </summary>
        /// <param name="moduleMount">The module mount where the new module was loaded.</param>
        protected override void OnModuleMounted(Module module)
        {
            // Get a reference to any Damageable on the new module 
            Health health = module.GetComponent<Health>();
            if (health == this) return;

            if (health != null)
            {
                foreach(Damageable damageable in health.damageables)
                {
                    AddDamageable(damageable);
                }
            }
            else
            {
                Damageable[] moduleDamageables = module.GetComponentsInChildren<Damageable>();
                foreach (Damageable damageable in moduleDamageables)
                {
                    AddDamageable(damageable);
                }
            }
        }


        /// <summary>
        /// Called every time a module is unmounted at a module mount.
        /// </summary>
        /// <param name="moduleMount">The module mount where the new module was unmounted.</param>
        protected override void OnModuleUnmounted(Module module)
        {
            // Get a reference to any Damageable on the new module 
            Health health = module.GetComponent<Health>();
            if (health == this) return;

            if (health != null)
            {
                foreach (Damageable damageable in health.damageables)
                {
                    RemoveDamageable(damageable);
                }
            }
            else
            {
                Damageable[] moduleDamageables = module.GetComponentsInChildren<Damageable>();
                foreach (Damageable damageable in moduleDamageables)
                {
                    RemoveDamageable(damageable);
                }
            }
        }
        
        /// <summary>
        /// Reset the health to starting conditions.
        /// </summary>
        public virtual void ResetHealth()
        {
            // Reset all of the damageables to starting conditions.
            foreach (Damageable damageable in damageables)
            {
                damageable.Restore();
            }
        }

        /// <summary>
        /// Get the maximum health for a specified health type.
        /// </summary>
        /// <param name="healthType">The health type being queried.</param>
        /// <returns>The maximum health.</returns>
        public virtual float GetMaxHealthByType(HealthType healthType)
        {
            float maxHealth = 0;

            for (int i = 0; i < damageables.Count; ++i)
            {
                if (damageables[i].HealthType == healthType)
                {
                    maxHealth += damageables[i].HealthCapacity;
                }
            }

            return maxHealth;
        }


        /// <summary>
        /// Get the current health for a specified health type.
        /// </summary>
        /// <param name="healthType">The health type being queried.</param>
        /// <returns>The current health.</returns>
        public virtual float GetCurrentHealthByType(HealthType healthType)
        {
            float currentHealth = 0;

            for (int i = 0; i < damageables.Count; ++i)
            {
                if (damageables[i].HealthType == healthType)
                {
                    currentHealth += damageables[i].CurrentHealth;
                }
            }

            return currentHealth;
        }

        /// <summary>
        /// Get the fraction of health remaining of a specified type.
        /// </summary>
        /// <param name="healthType">The health type.</param>
        /// <returns>The health fraction remaining</returns>
        public virtual float GetCurrentHealthFractionByType(HealthType healthType)
        {

            float currentHealth = 0;
            float maxHealth = 0.00001f;

            for (int i = 0; i < damageables.Count; ++i)
            {
                if (healthType == null || damageables[i].HealthType == healthType)
                {
                    currentHealth += damageables[i].CurrentHealth;
                    maxHealth += damageables[i].HealthCapacity;
                }
            }

            return currentHealth / maxHealth;
        }
    }
}