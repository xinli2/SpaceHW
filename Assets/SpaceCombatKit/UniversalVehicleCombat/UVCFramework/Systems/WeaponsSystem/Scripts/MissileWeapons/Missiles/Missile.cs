using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;
namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Base class for a guided missile.
    /// </summary>
    public class Missile : RigidbodyProjectile
    {
        [Header("Settings")]

        [SerializeField]
        protected float noLockLifetime = 4;

        [SerializeField]
        protected TargetProximityTriggerMode triggerMode = TargetProximityTriggerMode.OnDistanceIncrease;

        [SerializeField]
        protected float triggerDistance = 49;

        protected bool targetWasInsideTrigger = false;

        protected bool triggered = false;

        [Header("Guidance")]

        [SerializeField]
        protected PIDController3D steeringPIDController;

        [Header("Components")]

        [SerializeField]
        protected TargetLocker targetLocker;

        [SerializeField]
        protected VehicleEngines3D engines;   

        protected bool locked = false;


        public override float Speed
        {
            get { return engines.GetDefaultMaxSpeedByAxis(false).z; }
        }

        public override float Range
        {
            get { return targetLocker.LockingRange; }
        }

        public override float Damage(HealthType healthType)
        {
            for (int i = 0; i < healthModifier.DamageOverrideValues.Count; ++i)
            {
                if (healthModifier.DamageOverrideValues[i].HealthType == healthType)
                {
                    return healthModifier.DamageOverrideValues[i].Value;
                }
            }

            return healthModifier.DefaultDamageValue;
        }

        public override void AddVelocity(Vector3 addedVelocity)
        {
            base.AddVelocity(addedVelocity);
            m_Rigidbody.velocity += addedVelocity;
        }

        protected override void Reset()
        {

            base.Reset();

            m_Rigidbody.useGravity = false;
            m_Rigidbody.mass = 1;
            m_Rigidbody.drag = 3;
            m_Rigidbody.angularDrag = 4;

            // Add/get engines
            engines = transform.GetComponentInChildren<VehicleEngines3D>();
            if (engines == null)
            {
                engines = gameObject.AddComponent<VehicleEngines3D>();
            }

            // Add/get target locker
            targetLocker = transform.GetComponentInChildren<TargetLocker>();
            if (targetLocker == null)
            {
                targetLocker = gameObject.AddComponent<TargetLocker>();
            }

            detonator.DetonatingDuration = 2;

            disableAfterDistanceCovered = false;

            areaEffect = true;

            healthModifier.DefaultDamageValue = 1000;
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            targetWasInsideTrigger = false;
            triggered = false;
        }


        protected override void Awake()
        {
            base.Awake();

            if (collisionScanner != null)
            {
                collisionScanner.Rigidbody = m_Rigidbody;
            }
        }


        /// <summary>
        /// Set the target.
        /// </summary>
        /// <param name="target">The new target.</param>
        public virtual void SetTarget(Trackable target)
        {
            if (targetLocker != null)
            {
                targetLocker.SetTarget(target);
                if (target != null) targetLocker.SetLockState(LockState.Locked);
            }
        }

        /// <summary>
        /// Set the lock state of the missile.
        /// </summary>
        /// <param name="lockState">The new lock state.</param>
        public virtual void SetLockState(LockState lockState)
        {
            if (targetLocker != null) targetLocker.SetLockState(lockState);

            locked = true;
        }


        // Check if the trigger should be activated
        protected virtual void CheckTrigger()
        {
            if (triggered) return;

            if (targetLocker.Target == null) return;

            bool targetInsideTrigger = false;

            Collider[] colliders = Physics.OverlapSphere(transform.position, triggerDistance, collisionScanner.HitMask);
            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i].attachedRigidbody != null && colliders[i].attachedRigidbody == targetLocker.Target.Rigidbody)
                {
                    targetInsideTrigger = true;

                    bool triggerNow = false;

                    switch (triggerMode)
                    {
                        case TargetProximityTriggerMode.OnTargetInRange:

                            triggerNow = true;
                            break;

                        case TargetProximityTriggerMode.OnDistanceIncrease:

                            float dist0 = GetClosestDistanceToTarget(0, colliders[i]);
                            float dist1 = GetClosestDistanceToTarget(Time.deltaTime, colliders[i]);

                            triggerNow = Mathf.Abs(dist1 - dist0) > 0.01f && dist1 > dist0;

                            Vector3 toTarget = targetLocker.Target.transform.position - transform.position;
                            Vector3 toTargetNext = (targetLocker.Target.transform.position + (targetLocker.Target.Rigidbody == null ? Vector3.zero : targetLocker.Target.Rigidbody.velocity * Time.deltaTime)) -
                                                    (transform.position + (m_Rigidbody == null ? Vector3.zero : m_Rigidbody.velocity * Time.deltaTime));

                            if (toTargetNext.magnitude < toTarget.magnitude) triggerNow = false;

                            break;
                    }

                    if (triggerNow)
                    {
                        triggered = true;
                        Detonate();
                        return;
                    }
                }
            }

            if (!targetInsideTrigger && targetWasInsideTrigger)
            {
                triggered = true;
                Detonate();
            }
            else
            {
                targetWasInsideTrigger = targetInsideTrigger;
            }
        }


        // Get the closest distance to the target based on a time projection (necessary because things can move very fast and the target can
        // change position a lot in one frame).
        protected virtual float GetClosestDistanceToTarget(float timeProjection, Collider targetCollider)
        {
            Vector3 targetOffset = targetLocker.Target.Rigidbody != null ? targetLocker.Target.Rigidbody.velocity * timeProjection : Vector3.zero;

            if (m_Rigidbody != null) targetOffset -= m_Rigidbody.velocity * timeProjection;

            Vector3 closestPoint = targetCollider.ClosestPoint(transform.position + targetOffset);

            return (closestPoint - transform.position).magnitude;
        }


        protected override void Update()
        {
            base.Update();

            CheckTrigger();
            
            if (targetLocker.LockState == LockState.Locked)
            {
                if (engines != null)
                {
                    // Steer
                    Vector3 targetVelocity = targetLocker.Target.Rigidbody != null ? targetLocker.Target.Rigidbody.velocity : Vector3.zero;
                    Vector3 targetPos = TargetLeader.GetLeadPosition(transform.position, Speed, targetLocker.Target.transform.position, targetVelocity);
                    Maneuvring.TurnToward(transform, targetPos, new Vector3(360, 360, 0), steeringPIDController);
                    engines.SetSteeringInputs(steeringPIDController.GetControlValues());
                    engines.SetMovementInputs(new Vector3(0, 0, 1));
                }
                
            }
            else
            {
                // Detonate after lifetime
                if (locked)
                {
                    detonator.Detonate(noLockLifetime);
                    locked = false;
                }

                // Clear steering inputs
                if (engines != null)
                {
                    engines.SetSteeringInputs(Vector3.zero);
                    engines.SetMovementInputs(new Vector3(0, 0, 1));
                }
               
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Color c = Gizmos.color;

            Gizmos.color = new Color(1, 0, 0);
            Gizmos.DrawWireSphere(transform.position, triggerDistance);

            Gizmos.color = c;
        }

        protected override void MovementUpdate()
        {
            if (engines == null)
            {
                base.MovementUpdate();
            }
        }

        protected override void MovementFixedUpdate()
        {
            if (engines == null)
            {
                base.MovementFixedUpdate();
            }
        }
    }
}