using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Creates a directional thruster effect based on the steering and movement of a Vehicle Engines 3D component.
    /// </summary>
    public class DirectionalThrusterEffectBase : MonoBehaviour
    {

        [Tooltip("The engines component that this directional thruster references.")]
        [SerializeField]
        protected VehicleEngines3D engines;

        [Tooltip("The transform representing the position and orientation of the thruster.")]
        [SerializeField]
        protected Transform thrusterTransform;

        [Tooltip("The transform representing the center of mass of the vehicle.")]
        [SerializeField]
        protected Transform centerOfMass;

        protected float level;
        public float Level { get { return level; } }


        protected virtual void Reset()
        {
            thrusterTransform = transform;
            engines = transform.root.GetComponentInChildren<VehicleEngines3D>();
            if (engines != null) centerOfMass = engines.transform;
        }


        // Update is called once per frame
        protected virtual void Update()
        {
            Vector3 thrusterLocalPos = centerOfMass.InverseTransformPoint(thrusterTransform.position);
            Vector3 thrusterLocalDirection = centerOfMass.InverseTransformDirection(thrusterTransform.forward);

            Vector3 movement = engines.MovementInputs;
            float movementAmount = Mathf.Clamp(-Vector3.Dot(thrusterLocalDirection, movement), 0, 1);

            Vector3 rotationAxis = engines.SteeringInputs;
            Vector3 test = Vector3.ProjectOnPlane(thrusterLocalPos, thrusterLocalDirection).normalized;
            if (Mathf.Abs(test.x) > 0.01f) test.x = Mathf.Sign(test.x) * (test.x / test.x);
            if (Mathf.Abs(test.y) > 0.01f) test.y = Mathf.Sign(test.y) * (test.y / test.y);
            if (Mathf.Abs(test.z) > 0.01f) test.z = Mathf.Sign(test.z) * (test.z / test.z);

            float placementAmount = Mathf.Clamp(-Vector3.Dot(Vector3.Cross(rotationAxis, test), thrusterLocalDirection.normalized), 0, 1);

            level = Mathf.Min(movementAmount + placementAmount, 1);

        }
    }
}

