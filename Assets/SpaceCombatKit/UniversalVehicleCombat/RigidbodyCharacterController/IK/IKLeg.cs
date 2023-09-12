using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    [System.Serializable]
    public class IKLeg
    {
        [Tooltip("The position in space, relative to the upper leg joint, that the leg will bend toward.")]
        public Vector3 relativeJointTarget;

        [Header("Upper Leg")]

        [Tooltip("The IK joint for the upper leg.")]
        public IKJoint upperLeg;

        [Header("Lower Leg")]

        [Tooltip("The IK joint for the lower leg.")] 
        public IKJoint lowerLeg;

        [Header("Foot")]

        [Tooltip("The IK joint for the foot.")]
        public IKJoint foot;

        [Tooltip("The height between the ankle joint and the sole of the foot.")]
        public float footHeight;

        [HideInInspector]
        public Quaternion footRotation;

        [Header("Limits")]

        [Tooltip("The minimum extension (0-1) that the leg is allowed to reach.")]
        [Range(0, 1)]
        public float minExtension = 0.1f;

        [Tooltip("The maximum extension (0-1) that the leg is allowed to reach.")]
        [Range(0, 1)]
        public float maxExtension = 0.9f;

        public virtual float GetLength()
        {
            return (lowerLeg.m_Transform.position - upperLeg.m_Transform.position).magnitude + (foot.m_Transform.position - lowerLeg.m_Transform.position).magnitude;
        }
    }
}

