using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Links/unlinks damageable modules mounted at a module mount with damage receivers already on the vehicle.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class DamageableModuleLinker : MonoBehaviour
    {
        [Tooltip("The module mount to link damageable/healable modules from.")]
        [SerializeField]
        protected ModuleMount moduleMount;

        [Tooltip("Damage receivers (colliders that can receive damage/healing effects) on the vehicle.")]
        [SerializeField]
        protected List<DamageReceiver> damageReceivers = new List<DamageReceiver>();

        [Header("Events")]

        [Tooltip("Event called when a damageable module on the module mount is damaged.")]
        public OnDamageableDamagedEventHandler onDamageableModuleDamaged;

        [Tooltip("Event called when a damageable module on the module mount is healed.")]
        public OnDamageableHealedEventHandler onDamageableModuleHealed;

        [Tooltip("Event called when a damageable module on the module mount is destroyed.")]
        public OnDamageableDestroyedEventHandler onDamageableModuleDestroyed;

        [Tooltip("Event called when a damageable module on the module mount is restored after being destroyed.")]
        public OnDamageableRestoredEventHandler onDamageableModuleRestored;


        // Called when component is first added to a gameobject or reset in the inspector
        protected virtual void Reset()
        {
            moduleMount = GetComponent<ModuleMount>();
        }


        protected virtual void Awake()
        {
            moduleMount.onModuleMounted.AddListener(OnModuleMounted);
            moduleMount.onModuleUnmounted.AddListener(OnModuleUnmounted);
        }


        // Called when a module is mounted on the module mount
        protected virtual void OnModuleMounted(Module module)
        {
            Damageable damageable = module.GetComponent<Damageable>();
            if (damageable != null)
            {
                // Link this
                damageable.onDamaged.AddListener(OnDamageableModuleDamaged);
                damageable.onHealed.AddListener(OnDamageableModuleHealed);
                damageable.onDestroyed.AddListener(OnDamageableModuleDestroyed);
                damageable.onRestored.AddListener(OnDamageableModuleRestored);

                for (int i = 0; i < damageReceivers.Count; ++i)                {
                    // Link damage receivers
                    damageReceivers[i].SetDamageable(damageable);
                }
            }
        }


        // Called when a module is unmounted on the module mount
        protected virtual void OnModuleUnmounted(Module module)
        {
            Damageable damageable = module.GetComponent<Damageable>();
            if (damageable != null)
            {

                // Unlink this
                damageable.onDamaged.RemoveListener(OnDamageableModuleDamaged);
                damageable.onHealed.RemoveListener(OnDamageableModuleHealed);
                damageable.onDestroyed.RemoveListener(OnDamageableModuleDestroyed);
                damageable.onRestored.RemoveListener(OnDamageableModuleRestored);

                for (int i = 0; i < damageReceivers.Count; ++i)
                {
                    damageReceivers[i].SetDamageable(null);
                }
            }
        }


        // Function called when a damageable module on the module mount is damaged
        protected virtual void OnDamageableModuleDamaged(HealthEffectInfo info)
        {
            onDamageableModuleDamaged.Invoke(info);
        }


        // Function called when a damageable module on the module mount is healed
        protected virtual void OnDamageableModuleHealed(HealthEffectInfo info)
        {
            onDamageableModuleHealed.Invoke(info);
        }


        // Function called when a damageable module on the module mount is destroyed
        protected virtual void OnDamageableModuleDestroyed()
        {
            onDamageableModuleDestroyed.Invoke();
        }


        // Function called when a damageable module on the module mount is healed after being destroyed
        protected virtual void OnDamageableModuleRestored()
        {
            onDamageableModuleRestored.Invoke();
        }
    }
}
