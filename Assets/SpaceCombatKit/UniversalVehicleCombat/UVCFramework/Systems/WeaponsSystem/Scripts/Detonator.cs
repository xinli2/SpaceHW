using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VSX.Pooling;


namespace VSX.UniversalVehicleCombat
{

    public enum DetonationState
    {
        Reset,
        Detonating,
        Detonated
    }

    /// <summary>
    /// This class detonates an object (creates an explosion and deactivates the gameobject etc).
    /// </summary>
    public class Detonator : MonoBehaviour
    {

        protected DetonationState detonationState;
        public virtual DetonationState DetonationState { get { return detonationState; } }

        protected bool specifiedDetonationPosition = false;
        protected Vector3 detonationPosition;
        protected Quaternion detonationRotation;

        [SerializeField]
        protected bool usePoolManager;

        [Header("Detonating")]

        [SerializeField]
        protected List<GameObject> detonatingStateSpawnObjects = new List<GameObject>();
        public virtual List<GameObject> DetonatingStateSpawnObjects { get { return detonatingStateSpawnObjects; } }

        [SerializeField]
        protected float detonatingDuration = 0;
        public virtual float DetonatingDuration
        {
            get { return detonatingDuration; }
            set { detonatingDuration = value; }
        }

        [Header("Detonated")]

        [SerializeField]
        protected bool disableGameObjectOnDetonated = true;

        [SerializeField]
        protected List<GameObject> detonatedStateSpawnObjects = new List<GameObject>();
        public virtual List<GameObject> DetonatedStateSpawnObjects { get { return detonatedStateSpawnObjects; } }

        [Header("Timed Detonation")]

        [SerializeField]
        protected bool detonateAfterLifetime = false;

        [SerializeField]
        protected float lifeTime = 1;

        protected float lifeTimeStartTime;

        [Header("Events")]

        public UnityEvent onDetonating;

        // Detonator detonated event
        public UnityEvent onDetonated;    

        // Detonator reset event
        public UnityEvent onReset;


        protected virtual void OnDisable()
        {
            StopAllCoroutines();
        }

        protected virtual void OnEnable()
        {
            ResetDetonator();
            lifeTimeStartTime = Time.time;
        }

        protected virtual void Start()
        {
            if (usePoolManager && PoolManager.Instance == null)
            {
                usePoolManager = false;
                Debug.LogWarning("Cannot pool explosions or hit effects as there isn't a PoolManager in the scene. Please add one to use pooling, or set the usePoolManager field on this component to False.");
            }
        }


        protected virtual void SetDetonationState(DetonationState newState)
        {
            Vector3 thisDetonationPositon = specifiedDetonationPosition ? detonationPosition : transform.position;
            Quaternion thisDetonationRotation = specifiedDetonationPosition ? detonationRotation : transform.rotation;

            switch (newState)
            {
                case DetonationState.Detonating:

                    // Spawn Objects
                    for (int i = 0; i < detonatingStateSpawnObjects.Count; ++i)
                    {
                        if (usePoolManager)
                        {
                            PoolManager.Instance.Get(detonatingStateSpawnObjects[i], thisDetonationPositon, thisDetonationRotation);
                        }
                        else
                        {
                            Instantiate(detonatingStateSpawnObjects[i], thisDetonationPositon, thisDetonationRotation);
                        }
                    }

                    OnDetonating();

                    onDetonating.Invoke();

                    break;

                case DetonationState.Detonated:

                    // Spawn Objects
                    for (int i = 0; i < detonatedStateSpawnObjects.Count; ++i)
                    {
                        if (name.Contains("Mine"))Debug.Log(detonatedStateSpawnObjects[i].name);
                        if (usePoolManager)
                        {
                            PoolManager.Instance.Get(detonatedStateSpawnObjects[i], thisDetonationPositon, thisDetonationRotation);
                        }
                        else
                        {
                            Instantiate(detonatedStateSpawnObjects[i], thisDetonationPositon, thisDetonationRotation);
                        }
                    }

                    OnDetonated();

                    // Call the event
                    onDetonated.Invoke();

                    // Disable
                    if (disableGameObjectOnDetonated) gameObject.SetActive(false);

                    break;

                case DetonationState.Reset:

                    OnDetonatorReset();

                    // Call the event
                    onReset.Invoke();

                    break;
            }

            // Update the state
            detonationState = newState;
        }

        /// <summary>
        /// Detonate at the current position.
        /// </summary>
	    public virtual void Detonate()
        {
            Detonate(transform.position, transform.forward);
        }

        public virtual void Detonate(float delay)
        {
            if (gameObject.activeInHierarchy) StartCoroutine(DelayedDetonationCoroutine(delay));
        }

        /// <summary>
        /// Detonate at a raycast hit point.
        /// </summary>
        /// <param name="hit">The raycast hit information.</param>
        public virtual void Detonate(RaycastHit hit)
        {
            specifiedDetonationPosition = true;
            Detonate(hit.point, hit.normal, true);
        }

        /// <summary>
        /// Detonate at a world position.
        /// </summary>
        /// <param name="detonationPosition">The detonation position.</param>
        public virtual void Detonate(Vector3 detonationPosition, Vector3 detonationForward, bool specifiedDetonationPosition = false)
        {

            this.specifiedDetonationPosition = specifiedDetonationPosition;

            this.detonationPosition = detonationPosition;
            this.detonationRotation = Quaternion.LookRotation(detonationForward, transform.up);

            if (detonationState != DetonationState.Reset) return;

            transform.position = detonationPosition;

            // Start the coroutine
            if (gameObject.activeInHierarchy) StartCoroutine(DetonationCoroutine());

        }

        /// <summary>
        /// Reset the detonator.
        /// </summary>
        public virtual void ResetDetonator()
        {
            SetDetonationState(DetonationState.Reset);
        }

        protected virtual IEnumerator DelayedDetonationCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (detonationState == DetonationState.Reset)
            {
                Detonate();
            }
        }

        // Coroutine for detonation
        protected virtual IEnumerator DetonationCoroutine()
        {
            SetDetonationState(DetonationState.Detonating);
            if (detonatingDuration > 0) yield return new WaitForSeconds(detonatingDuration);
            SetDetonationState(DetonationState.Detonated);
        }

        protected virtual void OnDetonating() { }

        protected virtual void OnDetonated() { }

        protected virtual void OnDetonatorReset() { }


        protected virtual void Update()
        {
            if (detonateAfterLifetime)
            {
                if (Time.time - lifeTimeStartTime > lifeTime)
                {
                    Detonate(transform.position, transform.up);
                }
            }
        }
    }
}