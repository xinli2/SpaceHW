using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.ResourceSystem;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class implements engines (movement, steering and boost) for a space vehicle
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleEngines3D : Engines
	{
        [Header("Movement & Steering Forces")]

        [SerializeField]
        protected Rigidbody m_rigidbody;
        public virtual Rigidbody Rigidbody { get { return m_rigidbody; } }
        public virtual void SetRigidbodyKinematic(bool isKinematic)
        {
            m_rigidbody.isKinematic = isKinematic;
        }

        [Tooltip("The movement forces applied for each axis when full movement input (full throttle) is applied.")]
        [SerializeField]
        protected Vector3 maxMovementForces = new Vector3(400, 400, 400);
        public Vector3 MaxMovementForces
        {
            get { return maxMovementForces; }
        }

        [Tooltip("The steering forces applied for each axis when full steering input is applied.")]
        [SerializeField]
        protected Vector3 maxSteeringForces = new Vector3(16f, 16f, 25f);
        public Vector3 MaxSteeringForces
        {
            get { return maxSteeringForces; }
        }

        [Tooltip("The movement forces applied for each axis when boosting.")]
        [SerializeField]
        protected Vector3 maxBoostForces = new Vector3(800, 800, 800);
        public Vector3 MaxBoostForces
        {
            get { return maxBoostForces; }
        }

        [SerializeField]
        protected float movementInputResponseSpeed = 5;
        protected Vector3 currentMovementForcesByAxis = Vector3.zero;

        [Header("Speed-Steering Relationship")]
        [Tooltip("A curve that represents how much the player can steer (Y axis) relative to the amount of top speed the ship is going (X axis). Works for forward movement only.")]
        [SerializeField]
        protected AnimationCurve steeringBySpeedCurve = AnimationCurve.Linear(0, 1, 1, 1);

        [Tooltip("A coefficient that is multiplied by the steering during boost. Used instead of the Steering By Speed Curve when boost is activated.")]
        [SerializeField]
        protected float boostSteeringCoefficient = 1;

        [Header("Resource Handlers")]

        [SerializeField]
        protected List<ResourceHandler> boostResourceHandlers = new List<ResourceHandler>();



        /// Called when this component is first added to a gameobject or reset in the inspector
        protected virtual void Reset()
        {

            m_rigidbody = GetComponent<Rigidbody>();

            // Initialize the rigidbody with good values
            m_rigidbody.useGravity = false;
            m_rigidbody.mass = 1;
            m_rigidbody.drag = 3;
            m_rigidbody.angularDrag = 4;
        }

       
        protected virtual void Awake()
        {
            // Cache the rigidbody
            m_rigidbody = GetComponent<Rigidbody>();
        }


        /// <summary>
        /// Get the maximum speed on each axis, for example for loadout data.
        /// </summary>
        /// <param name="withBoost">Whether to include boost in the maximum speed.</param>
        /// <returns>The maximum speed on each axis.</returns>
        public override Vector3 GetDefaultMaxSpeedByAxis(bool withBoost)
		{
            Vector3 maxForces = maxMovementForces + (withBoost ? maxBoostForces : Vector3.zero);
            
			return (new Vector3(GetSpeedFromForce(maxForces.x, m_rigidbody), GetSpeedFromForce(maxForces.y, m_rigidbody), GetSpeedFromForce(maxForces.z, m_rigidbody)));

		}

        /// <summary>
        /// Get the current maximum speed on each axis, for example for normalizing speed indicators.
        /// </summary>
        /// <param name="withBoost">Whether to include boost in the maximum speed.</param>
        /// <returns>The maximum speed on each axis.</returns>
        public override Vector3 GetCurrentMaxSpeedByAxis(bool withBoost)
        {
            Vector3 maxForces = withBoost ? maxBoostForces : maxMovementForces;

            return (new Vector3(GetSpeedFromForce(maxForces.x, m_rigidbody), GetSpeedFromForce(maxForces.y, m_rigidbody), GetSpeedFromForce(maxForces.z, m_rigidbody)));

        }


        /// <summary>
        /// Calculate the maximum speed of this Rigidbody for a given force.
        /// </summary>
        /// <param name="force">The linear force to be used in the calculation.</param>
        /// <returns>The maximum speed.</returns>
        public static float GetSpeedFromForce(float force, Rigidbody rBody)
		{
            return (force / rBody.drag - force * Time.fixedDeltaTime) / rBody.mass; // Subtracting (force * Time.fixedDeltaTime) / rBody.mass because drag is applied AFTER force is added
        }
        
        /// <summary>
        /// Get the force required for the vehicle to end up at a specific speed 
        /// </summary>
        /// <param name="speed">The desired speed.</param>
        /// <returns>The force required to achieve that speed.</returns>
        public static float GetForceForSpeed(float speed, Rigidbody rBody)
        {
            return speed / ((1 / rBody.drag - Time.fixedDeltaTime) / rBody.mass);
        }

        public static float GetAngularSpeedFromTorque(float torque, Rigidbody rBody)
        {
            // NOTE this assumes torque is applied using ForceMode.Acceleration, which ignores the mass distribution and treats the rBody as a point mass
            return Mathf.Clamp(torque / Mathf.Max(rBody.angularDrag, 1) - torque * Time.fixedDeltaTime, -rBody.maxAngularVelocity, rBody.maxAngularVelocity);
        }

        public static float GetTorqueForAngularSpeed(float angularSpeed, Rigidbody rBody)
        {
            return angularSpeed / ((1 / Mathf.Max(rBody.angularDrag, 1) - Time.fixedDeltaTime));
        }

        /// <summary>
        /// Set the speed of the vehicle directly (will only work if the required force is within the max force limits.
        /// </summary>
        /// <param name="speedsByAxis">The speeds for the vehicle along each local axis.</param>
        public virtual void SetSpeed(Vector3 speedsByAxis)
        {
            SetMovementInputs(new Vector3(GetForceForSpeed(speedsByAxis.x, m_rigidbody) / maxMovementForces.x,
                                            GetForceForSpeed(speedsByAxis.y, m_rigidbody) / maxMovementForces.y,
                                            GetForceForSpeed(speedsByAxis.z, m_rigidbody) / maxMovementForces.z));
        }


        /// <summary>
        /// Set the speed of the ship on the local X axis.
        /// </summary>
        /// <param name="speed">The speed to set.</param>
        public virtual void SetSpeedXAxis(float speed)
        {
            SetMovementInputs(new Vector3((speed * m_rigidbody.mass * m_rigidbody.drag) / maxMovementForces.x, movementInputs.y, movementInputs.z));
        }

        /// <summary>
        /// Set the speed of the ship on the local Y axis.
        /// </summary>
        /// <param name="speed">The speed to set.</param>
        public virtual void SetSpeedYAxis(float speed)
        {
            SetMovementInputs(new Vector3(movementInputs.x, (speed * m_rigidbody.mass * m_rigidbody.drag) / maxMovementForces.y, movementInputs.z));
        }

        /// <summary>
        /// Set the speed of the ship on the local Z axis.
        /// </summary>
        /// <param name="speed">The speed to set.</param>
        public virtual void SetSpeedZAxis(float speed)
        {
            SetMovementInputs(new Vector3(movementInputs.x, movementInputs.y, (speed * m_rigidbody.mass * m_rigidbody.drag) / maxMovementForces.z));
        }


        protected virtual Vector3 GetCurrentMaxSteeringForces()
        {
            return maxSteeringForces;
        }


        protected virtual Vector3 GetCurrentMaxMovementForces()
        {
            return maxMovementForces;
        }


        protected virtual Vector3 GetCurrentMaxBoostForces()
        {
            return maxBoostForces;
        }

        public override void SetBoostInputs(Vector3 newValuesByAxis)
        {
            if (!Mathf.Approximately(boostInputs.magnitude, 0))
            {
                // Check if required resources are available
                for (int i = 0; i < boostResourceHandlers.Count; ++i)
                {
                    if (!boostResourceHandlers[i].Ready())
                    {
                        newValuesByAxis = Vector3.zero;
                        break;
                    }
                }
            }

            base.SetBoostInputs(newValuesByAxis);
        }


        // Called every frame
        protected virtual void Update()
        {
            // Use resources during boost
            if (enginesActivated && !Mathf.Approximately(boostInputs.magnitude, 0))
            {
                for (int i = 0; i < boostResourceHandlers.Count; ++i)
                {
                    if (boostResourceHandlers[i].Ready())
                    {
                        boostResourceHandlers[i].Implement();
                    }
                    else
                    {
                        base.SetBoostInputs(Vector3.zero);
                    } 
                }
            }
        }

        // Apply the physics
        protected virtual void FixedUpdate()
		{

            if (enginesActivated)
            {

                // Implement steering torques

                float steeringSpeedMultiplier = 1;
                if (boostInputs.z > 0.5f)
                {
                    steeringSpeedMultiplier = boostSteeringCoefficient;
                }
                else
                {
                    float topSpeed = GetCurrentMaxSpeedByAxis(false).z;
                    if (!Mathf.Approximately(topSpeed, 0))
                    {
                        float topSpeedAmount = Mathf.Clamp(Mathf.Abs(m_rigidbody.velocity.z / topSpeed), 0, 1);
                        steeringSpeedMultiplier = steeringBySpeedCurve.Evaluate(topSpeedAmount);
                    }
                }
               
                m_rigidbody.AddRelativeTorque(steeringSpeedMultiplier * Vector3.Scale(steeringInputs, GetCurrentMaxSteeringForces()), ForceMode.Acceleration);

                // Movement forces
                Vector3 nextMovementForces = Vector3.Scale(movementInputs, GetCurrentMaxMovementForces());

                if (boostInputs.x > 0.5f)
                    nextMovementForces.x = GetCurrentMaxBoostForces().x;
                if (boostInputs.y > 0.5f)
                    nextMovementForces.y = GetCurrentMaxBoostForces().y;
                if (boostInputs.z > 0.5f)
                    nextMovementForces.z = GetCurrentMaxBoostForces().z;

                nextMovementForces = Vector3.Lerp(currentMovementForcesByAxis, nextMovementForces, movementInputResponseSpeed * Time.deltaTime);
                currentMovementForcesByAxis = nextMovementForces;

                // Implement forces
                m_rigidbody.AddRelativeForce(nextMovementForces);

            }
		}
	}
}
