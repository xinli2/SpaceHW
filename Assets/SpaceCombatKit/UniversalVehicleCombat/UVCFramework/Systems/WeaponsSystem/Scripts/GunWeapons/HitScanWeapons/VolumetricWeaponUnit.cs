using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Controls a beam for e.g. a weapon.
    /// </summary>
    public class VolumetricWeaponUnit : WeaponUnit, IRootTransformUser
    {

        [Header("Hit Scan")]

        [SerializeField]
        protected Transform spawnPoint;
        public override void Aim(Vector3 aimPosition) { spawnPoint.LookAt(aimPosition, transform.up); }
        public override void ClearAim() { spawnPoint.localRotation = Quaternion.identity; }

        [SerializeField]
        protected float range = 1000;
        public override float Range { get { return range; } }

        public float radius = 5;

        [SerializeField]
        protected List<GameObject> hitPointSpawnObjects = new List<GameObject>();

        [Tooltip("The layer mask of colliders that can be hit.")]
        [SerializeField]
        protected LayerMask hitMask = Physics.DefaultRaycastLayers;

        [Tooltip("Whether to ignore trigger colliders.")]
        [SerializeField]
        protected bool ignoreTriggerColliders = true;

        [Tooltip("Whether to ignore collision with the object or vehicle that this object came from.")]
        [SerializeField]
        protected bool ignoreHierarchyCollision = true;

        [Header("Damage/Healing")]

        // Whether to apply the damage amount on a per-second basis (e.g. for laser beams).
        [SerializeField]
        protected bool timeBasedDamageHealing = false;
       
        [SerializeField]
        protected HealthModifier healthModifier;
        public HealthModifier HealthModifier { get { return healthModifier; } }

        public override float Damage(HealthType healthType)
        {

            return healthModifier.GetDamage(healthType);

        }

        [Header("Events")]

        public UnityEvent onHitScan;

        public RaycastHitEventHandler onHit;

        public UnityEvent onNoHit;

        protected Transform rootTransform;
        public Transform RootTransform
        {
            set { rootTransform = value; }
        }

        protected RaycastHit hit;

        protected bool firing = false;
        protected float firingStateChangeTime = -1000;

        public ParticleSystem m_ParticleSystem;
        protected ParticleSystem.EmissionModule emissionModule;

        public AudioSource m_Audio;
        public float maxVolume = 1;
        public float rampDuration = 0.3f;




        public override float Speed
        {
            get { return Mathf.Infinity; }
        }


        protected override void Reset()
        {
            base.Reset();
            spawnPoint = transform;
        }

        protected virtual void Awake()
        {
            m_Audio.volume = 0;

            emissionModule = m_ParticleSystem.emission;
            emissionModule.enabled = false;
        }

        // Called when scene starts
        protected virtual void Start()
        {
            if (rootTransform == null) rootTransform = transform.root;
        }


        // Do a hit scan
        public virtual void HitScan()
        {
            // Raycast
            RaycastHit[] hits;
            hits = Physics.SphereCastAll(spawnPoint.position, radius, spawnPoint.forward, range, hitMask, ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));    // Sort by distance

            for (int i = 0; i < hits.Length; ++i)
            {
                DamageReceiver damageReceiver = hits[i].collider.GetComponent<DamageReceiver>();
                
                if (ignoreHierarchyCollision && rootTransform != null && hits[i].collider.attachedRigidbody != null &&
                    hits[i].collider.attachedRigidbody.transform == rootTransform)
                {
                    continue;
                }

                OnHit(hits[i]);

                RaycastHitDamage(hits[i]);

            }

            onHitScan.Invoke();

            // No hits detected
            OnNoHit();
        }


        protected virtual void OnHit(RaycastHit hit)
        {
            for (int i = 0; i < hitPointSpawnObjects.Count; ++i)
            {
                Instantiate(hitPointSpawnObjects[i], hit.point, Quaternion.identity);
            }

            onHit.Invoke(hit);
        }

        protected virtual void OnNoHit()
        {
            onNoHit.Invoke();
        }

        public virtual void RaycastHitDamage(RaycastHit hit)
        {

            DamageReceiver damageReceiver = hit.collider.GetComponent<DamageReceiver>();

            if (damageReceiver != null)
            {
                // Damage

                HealthEffectInfo info = new HealthEffectInfo();
                info.worldPosition = hit.point;
                info.healthModifierType = healthModifier.HealthModifierType;
                info.sourceRootTransform = rootTransform;

                info.amount = healthModifier.GetDamage(damageReceiver.HealthType) * (timeBasedDamageHealing ? Time.deltaTime : 1);

                if (!Mathf.Approximately(info.amount, 0))
                {
                    damageReceiver.Damage(info);
                }

                // Healing

                info.amount = healthModifier.GetHealing(damageReceiver.HealthType) * (timeBasedDamageHealing ? Time.deltaTime : 1);

                if (!Mathf.Approximately(info.amount, 0))
                {
                    damageReceiver.Heal(info);
                }
            }
        }

        public override void StartTriggering()
        {
            firing = true;
            firingStateChangeTime = Time.time;
        }

        public override void StopTriggering()
        {
            firing = false;
            firingStateChangeTime = Time.time;
        }

        void Update()
        {
            m_ParticleSystem.Simulate(Time.deltaTime, true, false);
            if (firing)
            {
                emissionModule.enabled = true;
                HitScan();

                if ((Time.time - firingStateChangeTime) < rampDuration)
                {
                    float amount = (Time.time - firingStateChangeTime) / rampDuration;
                    m_Audio.volume = amount * maxVolume;
                }
                else
                {
                    m_Audio.volume = maxVolume;
                }
            }
            else
            {
                emissionModule.enabled = false;
                if ((Time.time - firingStateChangeTime) < rampDuration)
                {
                    float amount = (Time.time - firingStateChangeTime) / rampDuration;
                    m_Audio.volume = (1 - amount) * maxVolume;
                }
                else
                {
                    m_Audio.volume = 0;
                }
            }
        }
    }
}