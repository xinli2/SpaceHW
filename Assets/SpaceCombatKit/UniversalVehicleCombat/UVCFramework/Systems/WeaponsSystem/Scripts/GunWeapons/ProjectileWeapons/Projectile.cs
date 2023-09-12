using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.Pooling;


namespace VSX.UniversalVehicleCombat
{
    [System.Serializable]
    public class HitEffectsForSurfaceType
    {
        public List<GameObject> hitEffects = new List<GameObject>();
        public SurfaceType surfaceType;
    }

    /// <summary>
    /// Base class for a projectile.
    /// </summary>
    public class Projectile : MonoBehaviour
    {

        [Header("Damage/Healing")]

        [SerializeField]
        protected HealthModifier healthModifier;
        public HealthModifier HealthModifier { get { return healthModifier; } }

        [Tooltip("The amount of damage/healing as a function of distance covered / Max Distance")]
        [SerializeField]
        protected AnimationCurve healthEffectByDistanceCurve = AnimationCurve.Linear(0, 1, 1, 1);

        [Header("Hit Effects")]

        [Tooltip("The default hit effects for this projectile.")]
        [SerializeField]
        protected List<GameObject> defaultHitEffectPrefabs = new List<GameObject>();

        [Tooltip("Whether to spawn the hit effects when the projectile detonates (does not hit anything, just explodes).")]
        [SerializeField]
        protected bool spawnDefaultHitEffectsOnDetonation = true;

        [Tooltip("The hit effects to spawn when this projectile collides with specific surface types.")]
        [SerializeField]
        protected List<HitEffectsForSurfaceType> hitEffectOverrides = new List<HitEffectsForSurfaceType>();

        [Header("Area Effect Parameters")]

        [SerializeField]
        protected bool areaEffect = false;

        [SerializeField]
        protected float areaEffectRadius = 50;

        [SerializeField]
        protected AnimationCurve areaEffectFalloff = AnimationCurve.Linear(0, 1, 1, 0);

        [SerializeField]
        protected bool ignoreTriggerColliders = true;

        [SerializeField]
        protected LayerMask areaEffectLayerMask = ~0;

        [SerializeField]
        protected bool checkLineOfSight = true;


        [Header("Settings")]

        [SerializeField]
        protected CollisionScanner collisionScanner;

        [SerializeField]
        protected Detonator detonator;

        protected Transform senderRootTransform;
        protected List<IRootTransformUser> rootTransformUsers = new List<IRootTransformUser>();

        [SerializeField]
        protected float speed = 100;

        public enum MovementUpdateMode
        {
            Update,
            FixedUpdate
        }

        [SerializeField]
        protected MovementUpdateMode movementUpdateMode = MovementUpdateMode.FixedUpdate;


        [Header("Disable After Lifetime")]

        [SerializeField]
        protected bool disableAfterLifetime = false;

        [SerializeField]
        protected float lifetime = 3;
        protected float lifeStartTime;


        [Header("Disable After Distance")]

        [SerializeField]
        protected bool disableAfterDistanceCovered = true;
        protected Vector3 lastPosition;
        protected float distanceCovered = 0;

        [SerializeField]
        protected float maxDistance = 1000;

        protected List<TrailRenderer> trailRenderers = new List<TrailRenderer>();

        protected List<Renderer> renderers = new List<Renderer>();



        protected virtual void Reset()
        {
            collisionScanner = transform.GetComponent<CollisionScanner>();
            if (collisionScanner == null)
            {
                collisionScanner = gameObject.AddComponent<CollisionScanner>();
            }

            detonator = transform.GetComponent<Detonator>();
            if (detonator == null)
            {
                detonator = gameObject.AddComponent<Detonator>();
            }
        }


        protected virtual void Awake()
        {
            rootTransformUsers = new List<IRootTransformUser>(GetComponentsInChildren<IRootTransformUser>());

            if (collisionScanner != null) collisionScanner.onHitDetected.AddListener(OnCollision);

            trailRenderers = new List<TrailRenderer>(GetComponentsInChildren<TrailRenderer>(true));

            renderers = new List<Renderer>(GetComponentsInChildren<Renderer>(true));
        }


        public virtual void SetRendererLayers(int layer)
        {
            foreach(Renderer rend in renderers)
            {
                rend.gameObject.layer = layer;
            }
        }


        protected virtual void OnEnable()
        {
            lastPosition = transform.position;
            distanceCovered = 0;
            lifeStartTime = Time.time;

            foreach(TrailRenderer trailRenderer in trailRenderers)
            {
                trailRenderer.Clear();
            }
        }

        public virtual void Detonate()
        {
            
            if (areaEffect) AreaEffect();

            if (detonator != null) detonator.Detonate();

            if (spawnDefaultHitEffectsOnDetonation)
            {
                for(int i = 0; i < defaultHitEffectPrefabs.Count; ++i)
                {
                    if (PoolManager.Instance != null)
                    {
                        PoolManager.Instance.Get(defaultHitEffectPrefabs[i], transform.position, transform.rotation);
                    }
                    else
                    {
                        Instantiate(defaultHitEffectPrefabs[i], transform.position, transform.rotation);
                    }
                }
            }
        }

        /// <summary>
        /// Set the sender's root transform.
        /// </summary>
        /// <param name="senderRootTransform">The sender's root transform.</param>
        public virtual void SetSenderRootTransform(Transform senderRootTransform)
        {
            this.senderRootTransform = senderRootTransform;

            for (int i = 0; i < rootTransformUsers.Count; ++i)
            {
                rootTransformUsers[i].RootTransform = senderRootTransform;
            }
        }


        public virtual float Damage(HealthType healthType)
        {
            return healthModifier.GetDamage(healthType);
        }


        public virtual void AddVelocity(Vector3 addedVelocity) { }


        public virtual float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public virtual float Range
        {
            get { return Mathf.Min(disableAfterLifetime ? lifetime * Speed : Mathf.Infinity, disableAfterDistanceCovered ? maxDistance : Mathf.Infinity); }
        }

        protected virtual void OnCollision(RaycastHit hit)
        {

            DamageReceiver damageReceiver = null;

            if (areaEffect)
            {
                transform.position = hit.point;
                AreaEffect();
            }
            else
            {
                damageReceiver = hit.collider.GetComponent<DamageReceiver>();
                if (damageReceiver != null)
                {

                    HealthEffectInfo info = new HealthEffectInfo();
                    info.worldPosition = hit.point;
                    info.healthModifierType = healthModifier.HealthModifierType;
                    info.sourceRootTransform = senderRootTransform;

                    // Damage
                    info.amount = healthModifier.GetDamage(damageReceiver.HealthType) * healthEffectByDistanceCurve.Evaluate(distanceCovered / maxDistance);

                    if (!Mathf.Approximately(info.amount, 0))
                    {
                        damageReceiver.Damage(info);
                    }

                    // Healing

                    info.amount = healthModifier.GetHealing(damageReceiver.HealthType) * healthEffectByDistanceCurve.Evaluate(distanceCovered / maxDistance);

                    if (!Mathf.Approximately(info.amount, 0))
                    {
                        damageReceiver.Heal(info);
                    }
                }
            }
            
            if (damageReceiver != null)
            {
                CollisionHitEffects(hit, damageReceiver.SurfaceType);
            }
            else
            {
                CollisionHitEffects(hit, null);
            }

            if (detonator != null) detonator.Detonate(hit);
        }


        protected virtual void CollisionHitEffects(RaycastHit hit, SurfaceType surfaceType)
        {
            int effectOverrideIndex = -1;
            if (surfaceType != null)
            {
                for(int i = 0; i < hitEffectOverrides.Count; ++i)
                {
                    if (hitEffectOverrides[i].surfaceType == surfaceType)
                    {
                        effectOverrideIndex = i;
                        break;
                    }
                }
            }
            
            if (effectOverrideIndex == -1)
            {
                for (int i = 0; i < defaultHitEffectPrefabs.Count; ++i)
                {
                    if (PoolManager.Instance != null)
                    {
                        PoolManager.Instance.Get(defaultHitEffectPrefabs[i], hit.point, Quaternion.LookRotation(hit.normal));
                    }
                    else
                    {
                        Instantiate(defaultHitEffectPrefabs[i], hit.point, Quaternion.LookRotation(hit.normal));
                    }
                }
            }
            else
            {
                for (int i = 0; i < hitEffectOverrides[effectOverrideIndex].hitEffects.Count; ++i)
                {
                    if (PoolManager.Instance != null)
                    {
                        PoolManager.Instance.Get(hitEffectOverrides[effectOverrideIndex].hitEffects[i], hit.point, Quaternion.LookRotation(hit.normal));
                    }
                    else
                    {
                        Instantiate(hitEffectOverrides[effectOverrideIndex].hitEffects[i], hit.point, Quaternion.LookRotation(hit.normal));
                    }
                }
            }
        }


        protected virtual void AreaEffect()
        {
            if (!areaEffect) return;

            if (Mathf.Approximately(areaEffectRadius, 0)) return;

            // Get colliders in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, areaEffectRadius);
            
            // Track damageables already effected
            List<Damageable> hitDamageables = new List<Damageable>();

            for (int i = 0; i < colliders.Length; ++i)
            {
                // Ignore trigger colliders if that's checked
                if (ignoreTriggerColliders && colliders[i].isTrigger)
                {
                    continue;
                }
                
                // Check if the collider is on an area effect layer
                if (((1 << colliders[i].gameObject.layer) & areaEffectLayerMask) == 0)
                {
                    continue;
                }
                
                // Check line of sight if that's checked
                if (checkLineOfSight)
                {
                    RaycastHit hit;
                    Vector3 lineOfSightOrigin = transform.position - transform.forward * 0.01f;
                    if (Physics.Raycast(lineOfSightOrigin, (colliders[i].transform.position - lineOfSightOrigin).normalized, out hit, areaEffectRadius))
                    {
                        if (hit.collider != colliders[i]) continue;
                    }
                }
                
                DamageReceiver damageReceiver = colliders[i].GetComponent<DamageReceiver>();
                if (damageReceiver != null)
                {
                    // If damageable not already effected
                    if (hitDamageables.IndexOf(damageReceiver.Damageable) == -1)
                    {
                        // Get closest point 
                        Vector3 closestPoint = damageReceiver.GetClosestPoint(transform.position);

                        // Implement damage
                        if (Vector3.Distance(transform.position, closestPoint) < areaEffectRadius)
                        {
                            float distanceAmount = Vector3.Distance(transform.position, closestPoint) / areaEffectRadius;
                            float effectAmount = areaEffectFalloff.Evaluate(distanceAmount);

                            HealthEffectInfo info = new HealthEffectInfo();
                            info.worldPosition = transform.position;
                            info.healthModifierType = healthModifier.HealthModifierType;
                            info.sourceRootTransform = senderRootTransform;

                            // Damage

                            info.amount = healthModifier.GetDamage(damageReceiver.HealthType) * healthEffectByDistanceCurve.Evaluate(distanceCovered / maxDistance);

                            if (!Mathf.Approximately(info.amount, 0))
                            {
                                damageReceiver.Damage(info);
                            }

                            // Healing

                            info.amount = healthModifier.GetHealing(damageReceiver.HealthType) * healthEffectByDistanceCurve.Evaluate(distanceCovered / maxDistance);

                            if (!Mathf.Approximately(info.amount, 0))
                            {
                                damageReceiver.Heal(info);
                            }
                        }

                        // Add to list of already effected damageables
                        hitDamageables.Add(damageReceiver.Damageable);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }


        protected virtual void MovementUpdate()
        {
            if (detonator.DetonationState == DetonationState.Detonating || detonator.DetonationState == DetonationState.Detonated) return;
            transform.Translate(Vector3.forward * speed * (movementUpdateMode == MovementUpdateMode.Update ? Time.deltaTime : Time.fixedDeltaTime));
        }


        protected virtual void DisableProjectile()
        {
            gameObject.SetActive(false);
        }
        

        protected virtual void FixedUpdate()
        {
            if (movementUpdateMode == MovementUpdateMode.FixedUpdate) MovementUpdate();

        }


        protected virtual void Update()
        {
            if (movementUpdateMode == MovementUpdateMode.Update) MovementUpdate();

            distanceCovered += (transform.position - lastPosition).magnitude;

            if (disableAfterLifetime)
            {
                if (Time.time - lifeStartTime > lifetime)
                {
                    DisableProjectile();
                }
            }
            
            if (disableAfterDistanceCovered)
            {
                if (distanceCovered >= maxDistance)
                {
                    DisableProjectile();
                }
            }

            lastPosition = transform.position;
        }


        protected virtual void OnDrawGizmosSelected()
        {
            if (areaEffect)
            {
                Color c = Gizmos.color;

                Gizmos.color = new Color(1, 0.5f, 0);
                Gizmos.DrawWireSphere(transform.position, areaEffectRadius);

                Gizmos.color = c;
            }
        }
    }
}