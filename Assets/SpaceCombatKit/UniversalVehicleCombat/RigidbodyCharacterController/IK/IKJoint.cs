using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    [System.Serializable]
    public class IKJoint
    {
        [Tooltip("The transform that represents the joint (bone).")]
        public Transform m_Transform;

        [Tooltip("The rotation offset for the joint (bone) to transform from the IK orientation to the rigged orientation. IK calculations refer to the bone as pointing toward the next joint along the Z axis, with the Y axis pointed toward the joint target.")]
        public Vector3 rotationOffset;
    }
}

