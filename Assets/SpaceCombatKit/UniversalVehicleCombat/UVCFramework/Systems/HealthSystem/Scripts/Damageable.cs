using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// UnityEvent to run functions when a damageable is damaged.
    /// </summary>
    [System.Serializable]
    public class OnDamageableDamagedEventHandler : UnityEvent<HealthEffectInfo> { }

    /// <summary>
    /// UnityEvent to run functions when a damageable is healed.
    /// </summary>
    [System.Serializable]
    public class OnDamageableHealedEventHandler : UnityEvent<HealthEffectInfo> { }

    /// <summary>
    /// UnityEvent to run functions when a damageable is destroyed.
    /// </summary>
    [System.Serializable]
    public class OnDamageableDestroyedEventHandler : UnityEvent { }

    /// <summary>
    /// UnityEvent to run functions when a damageable is restored after being destroyed.
    /// </summary>
    [System.Serializable]
    public class OnDamageableRestoredEventHandler : UnityEvent { }


    [System.Serializable]
    public class CollisionEventHandler : UnityEvent<Collision> { }


    public struct HealthEffectInfo
    {
        public float amount;
        public Vector3 worldPosition;
        public HealthModifierType healthModifierType;
        public Transform sourceRootTransform;
    }


    /// <summary>
    /// Makes an object damageable and healable.
    /// </summary>
    public class Damageable : MonoBehaviour
    {

        [Header("General")]

        [SerializeField]
        protected string damageableID;
        public string DamageableID { get { return damageableID; } }

        // The health type of this damageable
        [SerializeField]
        protected HealthType healthType;
        public HealthType HealthType { get { return healthType; } }

        // The maximum health value for the container
        [SerializeField]
        protected float healthCapacity = 100;
        public virtual float HealthCapacity
        {
            get { return healthCapacity; }
            set
            {
                healthCapacity = value;
                healthCapacity = Mathf.Max(healthCapacity, 0);
                currentHealth = Mathf.Min(currentHealth, healthCapacity);
            }
        }

        // The health value of the container when the scene starts
        [SerializeField]
        protected float startingHealth = 100;
        public virtual float StartingHealth 
        { 
            get { return startingHealth; }
            set 
            { 
                startingHealth = Mathf.Clamp(value, 0, healthCapacity); 
            }
        }

        // The current health value of the container
        protected float currentHealth;
        public virtual float CurrentHealth { get { return currentHealth; } }
        public virtual float CurrentHealthFraction { get { return currentHealth / healthCapacity; } }

        public virtual void SetHealth(float newHealthValue)
        {
            currentHealth = Mathf.Clamp(newHealthValue, 0, healthCapacity);
            onHealthChanged.Invoke();
        }


        // Enable/disable damage
        [SerializeField]
        protected bool isDamageable = true;
        public virtual bool IsDamageable 
        { 
            get { return isDamageable; }
            set { isDamageable = value; }
        }

        // Enable/disable healing
        [SerializeField]
        protected bool isHealable = true;
        public virtual bool IsHealable 
        { 
            get { return isHealable; }
            set { isHealable = value; }
        }

        [SerializeField]
        protected bool canHealAfterDestroyed = false;

        [SerializeField]
        protected bool disableGameObjectOnDestroyed = true;

        [SerializeField]
        protected bool restoreOnEnable = true;


        [Header("Collisions")]

        [Tooltip("Whether to factor in the impulse (function of mass) of the collision when calculating damage.")]
        [SerializeField]
        protected bool applyImpulseDamageFactor = false;

        [Tooltip("The coefficient multiplied by the impulse (function of mass) of the collision that is multiplied to the damage value.")]
        [SerializeField]
        protected float collisionImpulseToDamageFactor = 0.5f;

        [SerializeField]
        protected HealthModifierType collisionHealthModifierType;

        [Tooltip("The maximum number of collision contacts allowed per collision. Damage is dealt for each contact point in a collision.")]
        [SerializeField]
        protected int collisionContactsLimit = 1;


        [Header("Events")]

        // Collision event
        public CollisionEventHandler onCollision;

        // Damageable damaged event
        public OnDamageableDamagedEventHandler onDamaged;

        // Damageable healed event
        public OnDamageableHealedEventHandler onHealed;

        // Damageable destroyed event
        public OnDamageableDestroyedEventHandler onDestroyed;

        // Damageable restored event
        public OnDamageableRestoredEventHandler onRestored;

        public UnityEvent onHealthChanged;

        // Whether this damageable is currently destroyed
        protected bool destroyed = false;
        public bool Destroyed { get { return destroyed; } }



        protected virtual void Awake()
        {
            SetHealth(startingHealth);
        }


        /// <summary>
        /// Restore when object is enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (restoreOnEnable) Restore(true);
        }

        /// <summary>
        /// Toggle whether this damageable is damageable.
        /// </summary>
        /// <param name="damageable">Whether this damageable is to be damageable.</param>
        public virtual void SetDamageable(bool isDamageable)
        {
            this.isDamageable = isDamageable;
        }


        /// <summary>
        /// Toggle whether this damageable is healable.
        /// </summary>
        /// <param name="healable">Whether this damageable is to be healable.</param>
        public void SetHealable(bool healable)
        {
            this.isHealable = healable;
        }


        protected virtual void OnCollisionEnter(Collision collision)
        {
            OnCollision(collision);
        }


        /// <summary>
        /// Called when a collision happens to check if it involves a one of this damageable's colliders (if so, damages it).
        /// </summary>
        /// <param name="collision">The collision information.</param>
        public virtual void OnCollision(Collision collision)
        {
            for (int i = 0; i < collision.contacts.Length; ++i)
            {
                if (i == collisionContactsLimit) { break; }

                HealthEffectInfo info = new HealthEffectInfo();
                info.amount = applyImpulseDamageFactor ? collisionImpulseToDamageFactor * collision.impulse.magnitude : 1;
                info.worldPosition = collision.contacts[i].point;
                info.healthModifierType = collisionHealthModifierType;
                Damage(info);

            }

            onCollision.Invoke(collision);
        }


        /// <summary>
        /// Damage this damageable.
        /// </summary>
        /// <param name="damage">The damage amount.</param>
        public virtual void Damage(float damage)
        {
            HealthEffectInfo info = new HealthEffectInfo();
            info.amount = damage;
            info.worldPosition = transform.position;
            
            Damage(info);
        }


        /// <summary>
        /// Damage this damageable.
        /// </summary>
        /// <param name="info">The damage information.</param>
        public virtual void Damage(HealthEffectInfo info)
        {

            if (destroyed) return;

            if (isDamageable)
            {
                if (!Mathf.Approximately(currentHealth, 0) && !Mathf.Approximately(info.amount, 0))
                {
                    // Reduce the health
                    currentHealth = Mathf.Clamp(currentHealth - info.amount, 0, healthCapacity);

                    // Call the damage event
                    onDamaged.Invoke(info);

                    // Destroy
                    if (Mathf.Approximately(currentHealth, 0))
                    {
                        Destroy();
                    }

                    onHealthChanged.Invoke();
                }
            }
        }

        
        /// <summary>
        /// Heal this damageable.
        /// </summary>
        /// <param name="healing">The healing amount.</param>
        public virtual void Heal(float healing)
        {
            HealthEffectInfo info = new HealthEffectInfo();
            info.amount = healing;
            info.worldPosition = transform.position;

            Heal(info);
        }


        /// <summary>
        /// Heal this damageable.
        /// </summary>
        /// <param name="info">The healing information.</param>
        public virtual void Heal(HealthEffectInfo info)
        {
            if (destroyed)
            {
                if (isHealable && canHealAfterDestroyed && info.amount > 0)
                {
                    Restore(false);
                }
                else
                {
                    return;
                }
            }

            if (isHealable)
            {
                if (!Mathf.Approximately(currentHealth, healthCapacity) && !Mathf.Approximately(info.amount, 0))
                {
                    // Add the health
                    currentHealth = Mathf.Clamp(currentHealth + info.amount, 0, healthCapacity);

                    onHealed.Invoke(info);

                    onHealthChanged.Invoke();
                }
            }
        }


        /// <summary>
        /// Destroy this damageable.
        /// </summary>
        public void Destroy()
        {
            // If already in the correct state, return
            if (destroyed) return;

            destroyed = true;

            // Call the destroyed event
            onDestroyed.Invoke();

            if (disableGameObjectOnDestroyed) gameObject.SetActive(false);

        }

        /// <summary>
        /// Restore this damageable.
        /// </summary>
        /// <param name="reset">Whether to reset to starting conditions.</param>
        public void Restore(bool reset = true)
        {

            destroyed = false;

            gameObject.SetActive(true);

            if (reset)
            {
                currentHealth = healthCapacity;
            }
            
            // Call the event
            onRestored.Invoke();
            
        }


        public virtual void SetColliderActivation(bool activate) { }
    }
}