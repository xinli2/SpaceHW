using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.CameraSystem
{
    [System.Serializable]
    public class CameraViewParentingItem
    {
        [Tooltip("The object that must change parent in different camera views.")]
        public Transform m_Transform;

        [Tooltip("The camera views in which the object will be parented to the specified parent transform.")]
        public List<CameraView> cameraViews = new List<CameraView>();

        [Tooltip("The type of parenting to implement.")]
        public CameraViewParentType parentType;

        [Tooltip("The parent transform to parent to.")]
        public Transform parentTransform;

        [Tooltip("Whether to set the local position to a specified value when this parenting event occurs.")]
        public bool setLocalPosition = true;

        [Tooltip("The local position to set the object to when this parenting event occurs.")]
        public Vector3 localPosition = Vector3.zero;

        [Tooltip("Whether to set the local rotation to a specified value when this parenting event occurs.")]
        public bool setLocalRotation = true;

        [Tooltip("The local rotation to set the object to when this parenting event occurs.")]
        public Vector3 localRotationEulerAngles = Vector3.zero;

        [Tooltip("Whether to set the local scale to a specified value when this parenting event occurs.")]
        public bool setLocalScale = false;

        [Tooltip("The local scele to set the object to when this parenting event occurs.")]
        public Vector3 localScale = new Vector3(1, 1, 1);
    }

}
