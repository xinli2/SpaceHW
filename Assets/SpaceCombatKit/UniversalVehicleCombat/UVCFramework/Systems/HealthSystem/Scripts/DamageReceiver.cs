using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Unity event for running functions when the damage receiver is damaged.
    /// </summary>
    [System.Serializable]
    public class OnDamageReceiverDamagedEventHandler : UnityEvent<HealthEffectInfo> { }

    /// <summary>
    /// Unity event for running functions when the damage receiver is healed.
    /// </summary>
    [System.Serializable]
    public class OnDamageReceiverHealedEventHandler : UnityEvent<HealthEffectInfo> { }

    /// <summary>
    /// Unity event for running functions when the damage receiver experiences a collision
    /// </summary>
    [System.Serializable]
    public class OnCollisionEventHandler : UnityEvent<Collision> { }

    /// <summary>
    /// Unity event for running functions when the damage receiver is activated
    /// </summary>
    [System.Serializable]
    public class OnDamageReceiverActivatedEventHandler : UnityEvent { }

    /// <summary>
    /// Unity event for running functions when the damage receiver is deactivated
    /// </summary>
    [System.Serializable]
    public class OnDamageReceiverDeactivatedEventHandler : UnityEvent { }



    /// <summary>
    /// This class makes an object damageable and healable.
    /// </summary>
    public class DamageReceiver : MonoBehaviour
    {

        // All the colliders on this gameobject
        protected List<Collider> colliders = new List<Collider>();
        public List<Collider> Colliders { get { return colliders; } }

        [SerializeField]
        protected Damageable damageable;
        public Damageable Damageable { get { return damageable; } }

        [SerializeField]
        protected bool disableCollidersOnDamageableDestroyed = true;

        // The root gameobject of this damageable
        [SerializeField]
        private Transform rootTransform;
        public Transform RootTransform { get { return rootTransform; } }

        [SerializeField]
        protected SurfaceType surfaceType;
        public SurfaceType SurfaceType { get { return surfaceType; } }

        public virtual HealthType HealthType { get { return damageable == null ? null : damageable.HealthType; } }

        [Header("Events")]

        // Damage event
        public OnDamageReceiverDamagedEventHandler onDamaged;

        // Heal event
        public OnDamageReceiverHealedEventHandler onHealed;

        // Collision event
        public OnCollisionEnterEventHandler onCollision;

        // Activated event
        public OnDamageReceiverActivatedEventHandler onActivated;

        // Deactivated event
        public OnDamageReceiverDeactivatedEventHandler onDeactivated;

        


        // Called when this component is first added to a gameobject, or reset in the inspector
        protected virtual void Reset()
        {
            rootTransform = transform.root;

            // Look for the first damageable at the same level or lower in the hierarchy
            damageable = transform.GetComponentInChildren<Damageable>();

            // Look for the first damageable anywhere in the hierarchy
            if (damageable == null) damageable = transform.root.GetComponentInChildren<Damageable>();
        }
        
        protected virtual void Awake()
        {
            // Get the colliders
            colliders = new List<Collider>(transform.GetComponents<Collider>());
            if (colliders.Count > 0)
            {
                if (colliders[0].attachedRigidbody != null)
                {
                    rootTransform = colliders[0].attachedRigidbody.transform;
                }
            }

            if (rootTransform == null) rootTransform = transform.root;

            if (damageable != null)
            {
                SetDamageable(damageable);
            }
        }


        public virtual void SetDamageable(Damageable damageable)
        {
            // Unlink previous damageable
            if (this.damageable != null)
            {
                onDamaged.RemoveListener(this.damageable.Damage);
                onHealed.RemoveListener(this.damageable.Heal);
                onCollision.RemoveListener(this.damageable.OnCollision);

                if (disableCollidersOnDamageableDestroyed)
                {
                    this.damageable.onDestroyed.RemoveListener(() => SetActivation(false));
                    this.damageable.onRestored.RemoveListener(() => SetActivation(true));
                }
            }

            // Set new damageable
            this.damageable = damageable;

            // Link new damageable
            if (damageable != null)
            {
                onDamaged.AddListener(damageable.Damage);
                onHealed.AddListener(damageable.Heal);

                onCollision.AddListener(damageable.OnCollision);

                if (disableCollidersOnDamageableDestroyed)
                {
                    this.damageable.onDestroyed.AddListener(() => SetActivation(false));
                    this.damageable.onRestored.AddListener(() => SetActivation(true));
                }
            }
        }

        /// <summary>
        /// Check for a collision with one of this damage receiver's colliders.
        /// </summary>
        /// <param name="collision">The collision.</param>
        public virtual void OnCollision(Collision collision)
        {
            for (int i = 0; i < collision.contacts.Length; ++i)
            {
                for (int j = 0; j < colliders.Count; ++j)
                {
                    if (collision.contacts[i].thisCollider == colliders[j])
                    {
                        // Call the event
                        onCollision.Invoke(collision);
                        return;
                    }
                }
            }
        }


        /// <summary>
        /// Send damage to this damage receiver.
        /// </summary>
        /// <param name="damage">The damage amount.</param>
        public virtual void Damage(float damage)
        {
            HealthEffectInfo info = new HealthEffectInfo();
            info.amount = damage;
            info.worldPosition = damageable.transform.position;

            Damage(info);
        }


        /// <summary>
        /// Damage the damageable this receiver is connected to.
        /// </summary>
        /// <param name="info">The damage information.</param>
        public virtual void Damage(HealthEffectInfo info)
        {
            onDamaged.Invoke(info);
        }


        /// <summary>
        /// Send healing to the damageable connected to this receiver.
        /// </summary>
        /// <param name="healing">The healing amount.</param>
        public virtual void Heal(float healing)
        {
            HealthEffectInfo info = new HealthEffectInfo();
            info.amount = healing;
            info.worldPosition = damageable.transform.position;

            Damage(healing);
        }


        /// <summary>
        /// Health the damageable this receiver is connected to.
        /// </summary>
        /// <param name="info">The healing information.</param>
        public virtual void Heal(HealthEffectInfo info)
        {
            onHealed.Invoke(info);
        }


        /// <summary>
        /// Enable or disable the colliders.
        /// </summary>
        /// <param name="collidersEnabled">Whether the colliders should be enabled or disabled.</param>
        public virtual void SetActivation(bool activated)
        {
            for (int i = 0; i < colliders.Count; ++i)
            {
                colliders[i].enabled = activated;
            }

            if (activated)
            {
                onActivated.Invoke();
            }
            else
            {
                onDeactivated.Invoke();
            }
        }


        /// <summary>
        /// Get the closest point on any of the colliders to a given world space position;
        /// </summary>
        /// <param name="position">The world space reference position.</param>
        /// <returns>The closest point.</returns>
        public virtual Vector3 GetClosestPoint(Vector3 position)
        {

            // If no colliders, use this gameobject position
            if (colliders.Count == 0)
            {
                return transform.position;
            }
            else if (colliders.Count == 1)
            {
                return colliders[0].ClosestPoint(position);
            }

            // Cache the closest point and the distance of the closest point
            Vector3 closestPoint = colliders[0].ClosestPoint(position);
            float distance = Vector3.Distance(closestPoint, position);

            // Go through all of the colliders and update the closest point and distance
            for (int i = 1; i < colliders.Count; ++i)
            {
                Vector3 thisClosestPoint = colliders[i].ClosestPoint(position);
                float thisDistance = Vector3.Distance(thisClosestPoint, position);
                if (thisDistance < distance)
                {
                    closestPoint = thisClosestPoint;
                    distance = Vector3.Distance(thisClosestPoint, position);
                }
            }

            return closestPoint;
        }
    }
}