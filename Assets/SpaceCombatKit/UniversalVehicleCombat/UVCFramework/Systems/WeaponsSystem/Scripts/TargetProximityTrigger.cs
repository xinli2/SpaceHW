using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VSX.UniversalVehicleCombat.Radar;


namespace VSX.UniversalVehicleCombat
{
 
    /// <summary>
    /// Triggers when a target enters a trigger collider, with the option to only trigger when the distance is projected to increase
    /// in the next frame.
    /// </summary>
    public class TargetProximityTrigger : MonoBehaviour
    {
        
        [Header("Settings")]

        [SerializeField]
        protected Rigidbody m_rigidbody;

        [SerializeField]
        protected Trackable target;
        public virtual Trackable Target
        {
            set 
            { 
                target = value;
                if (target != null)
                {
                    targetRigidbody = target.GetComponent<Rigidbody>();
                }
            }
        }
        protected Rigidbody targetRigidbody;

        [SerializeField]
        protected TargetProximityTriggerMode triggerMode = TargetProximityTriggerMode.OnDistanceIncrease;

        [SerializeField]
        protected float triggerDistance = 100;

        protected bool targetWasInsideTrigger = false;

        protected bool triggered = false;

        [Header("Events")]

        // Proximity trigger triggered event
        public UnityEvent onTriggered;



        protected virtual void Reset()
        {
            m_rigidbody = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            triggered = false;
            targetWasInsideTrigger = false;
        }

        // Check if the trigger should be activated
        protected virtual void CheckTrigger()
        {

            if (triggered || target == null) return;

            bool targetInsideTrigger = false;

            Collider[] colliders = Physics.OverlapSphere(transform.position, triggerDistance);
            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i].attachedRigidbody != null && colliders[i].attachedRigidbody.transform == target.transform)
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

                            break;
                    }

                    if (triggerNow)
                    {
                        onTriggered.Invoke();
                        triggered = true;
                        return;
                    }
                }
            }

            if (!targetInsideTrigger && targetWasInsideTrigger)
            {
                onTriggered.Invoke();
                triggered = true;
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
            Vector3 targetOffset = target.Rigidbody != null ? target.Rigidbody.velocity * timeProjection : Vector3.zero;

            if (m_rigidbody != null) targetOffset -= m_rigidbody.velocity * timeProjection;

            Vector3 closestPoint = targetCollider.ClosestPoint(transform.position + targetOffset);
            
            return (closestPoint - transform.position).magnitude;
        }


        // Called every frame
        protected virtual void Update()
        {
            CheckTrigger();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Color c = Gizmos.color;

            Gizmos.color = new Color(1, 0, 0);
            Gizmos.DrawWireSphere(transform.position, triggerDistance);

            Gizmos.color = c;
        }
    }
}