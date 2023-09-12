using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Manage a set of Damageables as a single entity.
    /// </summary>
    public class Damageables : MonoBehaviour
    {

        [Tooltip("All the damageables to manage with this component.")]
        [SerializeField] 
        protected List<Damageable> damageables = new List<Damageable>();
        public List<Damageable> DamageablesList { get { return damageables; } }


        [Tooltip("Event called when all the damageables are destroyed.")]
        public UnityEvent onDestroyed;


        [Tooltip("Event called when all the damageables are restored after being destroyed.")]
        public UnityEvent onRestored;



        protected virtual void Awake()
        {
            foreach(Damageable damageable in damageables)
            {
                damageable.onDestroyed.AddListener(CheckDestroyed);
                damageable.onRestored.AddListener(CheckRestored);
            }
        }

        /// <summary>
        /// Check if the damageables have all been destroyed.
        /// </summary>
        public virtual void CheckDestroyed()
        {
            foreach (Damageable damageable in damageables)
            {
                if (!damageable.Destroyed) return;
            }

            onDestroyed.Invoke();
        }

        /// <summary>
        /// Check if the damageables have all been restored.
        /// </summary>
        protected virtual void CheckRestored()
        {
            foreach (Damageable damageable in damageables)
            {
                if (damageable.Destroyed) return;
            }

            onRestored.Invoke();
        }

        /// <summary>
        /// Destroy all the damageables.
        /// </summary>
        public virtual void Destroy()
        {
            foreach (Damageable damageable in damageables)
            {
                damageable.Destroy();
            }
        }

        /// <summary>
        /// Restore all the damageables.
        /// </summary>
        public virtual void Restore()
        {
            foreach (Damageable damageable in damageables)
            {
                damageable.Restore();
            }
        }
    }
}
