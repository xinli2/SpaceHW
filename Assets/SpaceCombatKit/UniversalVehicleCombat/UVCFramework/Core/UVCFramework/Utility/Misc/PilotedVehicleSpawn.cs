using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VSX.Pooling;
using VSX.UniversalVehicleCombat.Loadout;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Spawns a pilot (Game Agent) and a vehicle in the scene.
    /// </summary>
    public class PilotedVehicleSpawn : MonoBehaviour
    {
        [Header("General")]

        [Tooltip("Whether to create the pilot and vehicle as soon as the scene starts.")]
        [SerializeField]
        protected bool spawnOnStart = true;
        
        [Tooltip("The transform that represents the position and rotation of the vehicle when it is spawned.")]
        [SerializeField]
        protected Transform spawnTransform;
        public virtual Transform SpawnTransform { get { return spawnTransform; } }

        [Tooltip("Whether to respawn after a specified time after the vehicle is destroyed.")]
        [SerializeField]
        protected bool respawnAfterTime = true;

        [Tooltip("How long to wait after the vehicle is destroyed to respawn.")]
        [SerializeField]
        protected float respawnTime = 3;

        protected float respawnStartTime;

        [Tooltip("Whether to use the Pool Manager to create the pilot and vehicle.")]
        [SerializeField]
        protected bool usePool = false;

        protected bool created = false;

        [Header("Pilot")]

        [Tooltip("A reference to the pilot of the vehicle. May be a prefab or already present in the scene.")]
        [SerializeField]
        protected GameAgent pilotReference;

        [Tooltip("Whether to instantiate the pilot. Be sure to set this to True if the pilot reference is a prefab.")]
        [SerializeField]
        protected bool instantiatePilot = true;

        [Tooltip("Whether to parent the pilot to the spawn transform.")]
        [SerializeField]
        protected bool parentPilotToSpawnTransform = true;

        protected GameAgent spawnedPilot;
        public GameAgent Pilot { get { return spawnedPilot; } }

        [SerializeField]
        protected bool specifyLoadout = false;

        [SerializeField]
        protected List<int> loadout = new List<int>();

        [SerializeField]
        protected LoadoutItems loadoutItems;


        [Header("Vehicle")]

        [Tooltip("A reference to the vehicle. May be a prefab or already present in the scene.")]
        [SerializeField]
        protected Vehicle vehicleReference;

        [Tooltip("Whether to instantiate the vehicle. Be sure to set this to True if the vehicle reference is a prefab.")]
        [SerializeField]
        protected bool instantiateVehicle = true;

        [Tooltip("Whether to parent the vehicle to the spawn transform.")]
        [SerializeField]
        protected bool parentVehicleToSpawnTransform = true;

        protected Vehicle spawnedVehicle;
        public Vehicle Vehicle { get { return spawnedVehicle; } }

        protected bool spawned = false;
        public bool Spawned { get { return spawned; } }

        protected bool destroyed = false;
        public virtual bool Destroyed
        {
            get { return destroyed; }
        }

        [Header("Events")]

        public UnityEvent onSpawned;
        public UnityEvent onDestroyed;


        protected virtual void Reset()
        {
            // Initialize the spawn transform
            spawnTransform = transform;
        }


        protected virtual void Start()
        {
            if (spawnOnStart) Spawn();
        }


        protected virtual void OnVehicleDestroyed()
        {
            destroyed = true;
            onDestroyed.Invoke();
            respawnStartTime = Time.time;
        }


        /// <summary>
        /// Create the pilot and the vehicle.
        /// </summary>
        public virtual void Spawn()
        {
            if (instantiatePilot)
            {
                if (usePool && PoolManager.Instance != null)
                {
                    spawnedPilot = PoolManager.Instance.Get(pilotReference.gameObject, Vector3.zero, Quaternion.identity, parentPilotToSpawnTransform ? spawnTransform : null).GetComponent<GameAgent>();
                    
                }
                else
                {
                    if (!created)
                    {
                        spawnedPilot = Instantiate(pilotReference, parentPilotToSpawnTransform ? spawnTransform : null);
                        spawnedPilot.transform.localPosition = Vector3.zero;
                        spawnedPilot.transform.localRotation = Quaternion.identity;
                    }
                }
            }
            else
            {
                spawnedPilot = pilotReference;
            }

            if (instantiateVehicle)
            {
                if (usePool && PoolManager.Instance != null)
                {
                    spawnedVehicle = PoolManager.Instance.Get(vehicleReference.gameObject, spawnTransform.position, spawnTransform.rotation, parentVehicleToSpawnTransform ? spawnTransform : null).GetComponent<Vehicle>();
                }
                else
                {
                    if (!created)
                    {
                        spawnedVehicle = Instantiate(vehicleReference, parentVehicleToSpawnTransform ? spawnTransform : null);
                        spawnedVehicle.transform.localPosition = Vector3.zero;
                        spawnedVehicle.transform.localRotation = Quaternion.identity;
                    }
                }
            }
            else
            {
                spawnedVehicle = vehicleReference;
            }

            if (spawnedPilot.IsDead) spawnedPilot.Revive();
            if (spawnedVehicle.Destroyed) spawnedVehicle.Restore();
            spawnedVehicle.gameObject.SetActive(true);

            spawnedVehicle.CachedRigidbody.velocity = Vector3.zero;
            spawnedVehicle.CachedRigidbody.angularVelocity = Vector3.zero;

            destroyed = false;

            if (spawnedPilot.Vehicle != spawnedVehicle) spawnedPilot.EnterVehicle(spawnedVehicle);

            created = true;

            spawnedVehicle.onDestroyed.RemoveListener(OnVehicleDestroyed);
            spawnedVehicle.onDestroyed.AddListener(OnVehicleDestroyed);

            spawned = true;

            onSpawned.Invoke();

        }


        /// <summary>
        /// In the case of an animated spawn, skip to the end and finish.
        /// </summary>
        public virtual void FinishSpawn() { }


        protected void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(spawnTransform.position, 10);
        }

        protected virtual void Update()
        {
            if (destroyed && respawnAfterTime && Time.time - respawnStartTime >= respawnTime)
            {
                Spawn();
            }
        }
    }
}

