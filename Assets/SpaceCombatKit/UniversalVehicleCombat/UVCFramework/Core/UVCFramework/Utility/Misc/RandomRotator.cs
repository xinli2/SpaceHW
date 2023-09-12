using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Trigger a random rotation with specified limits around each axis.
    /// </summary>
    public class RandomRotator : MonoBehaviour
    {

        [Tooltip("The transform to rotate.")]
        [SerializeField]
        protected Transform targetTransform;

        [Tooltip("Whether to add a random rotation when this component becomes enabled in the scene.")]
        [SerializeField]
        protected bool rotateOnEnable = true;


        [Header("Rotation Limits X")]

        [Tooltip("The minimum (positive or negative) rotation around the X axis.")]
        [SerializeField]
        protected float minRotationX = 0;

        [Tooltip("The maximum (positive or negative) rotation around the X axis.")]
        [SerializeField]
        protected float maxRotationX = 0;


        [Header("Rotation Limits Y")]

        [Tooltip("The minimum (positive or negative) rotation around the Y axis.")]
        [SerializeField]
        protected float minRotationY = 0;

        [Tooltip("The maximum (positive or negative) rotation around the Y axis.")]
        [SerializeField]
        protected float maxRotationY = 0;


        [Header("Rotation Limits Z")]

        [Tooltip("The minimum (positive or negative) rotation around the Z axis.")]
        [SerializeField]
        protected float minRotationZ = 0;

        [Tooltip("The maximum (positive or negative) rotation around the Z axis.")]
        [SerializeField]
        protected float maxRotationZ = 0;


        protected virtual void Reset()
        {
            targetTransform = transform;
        }


        protected virtual void OnEnable()
        {
            if (rotateOnEnable)
            {
                NewRotation();
            }
        }


        /// <summary>
        /// Implement a new random rotation.
        /// </summary>
        public virtual void NewRotation()
        {

            targetTransform.localRotation = Quaternion.Euler(Random.Range(minRotationX, maxRotationX),
                                                        Random.Range(minRotationY, maxRotationY),
                                                        Random.Range(minRotationZ, maxRotationZ));
        }
    }
}