using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Trigger a random scaling along each axis.
    /// </summary>
    public class RandomScaler : MonoBehaviour
    {

        [Tooltip("The transform to scale.")]
        [SerializeField]
        protected Transform targetTransform;

        [Tooltip("Whether to add a random scale when the component becomes enabled in the scene.")]
        [SerializeField]
        protected bool scaleOnEnable = true;


        [Header("Scale Limits X")]

        [Tooltip("The minimum scale on the X axis.")]
        [SerializeField]
        protected float minScaleX = 0;

        [Tooltip("The maximum scale on the X axis.")]
        [SerializeField]
        protected float maxScaleX = 0;


        [Header("Scale Limits Y")]

        [Tooltip("The minimum scale on the Y axis.")]
        [SerializeField]
        protected float minScaleY = 0;

        [Tooltip("The maximum scale on the Y axis.")]
        [SerializeField]
        protected float maxScaleY = 0;


        [Header("Scale Limits Z")]

        [Tooltip("The minimum scale on the Z axis.")]
        [SerializeField]
        protected float minScaleZ = 0;

        [Tooltip("The maximum scale on the Z axis.")]
        [SerializeField]
        protected float maxScaleZ = 0;


        protected virtual void Reset()
        {
            targetTransform = transform;
        }


        protected virtual void OnEnable()
        {
            if (scaleOnEnable)
            {
                NewScale();
            }
        }

        /// <summary>
        /// Implement a new random scale.
        /// </summary>
        public virtual void NewScale()
        {

            transform.localScale = new Vector3(Random.Range(minScaleX, maxScaleX),
                                                        Random.Range(minScaleY, maxScaleY),
                                                        Random.Range(minScaleZ, maxScaleZ));
        }
    }
}