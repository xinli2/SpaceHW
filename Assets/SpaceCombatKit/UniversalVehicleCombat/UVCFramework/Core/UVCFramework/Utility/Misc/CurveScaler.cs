using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.Utilities
{
    /// <summary>
    /// Scale an object using an animation curve.
    /// </summary>
    public class CurveScaler : MonoBehaviour
    {

        [Tooltip("The transform being animated.")]
        [SerializeField]
        protected Transform animatedTransform;

        [Tooltip("The duration of the animation.")]
        [SerializeField]
        protected float animationTime;

        [Tooltip("The curve describing the scale over the duration of the animation.")]
        [SerializeField]
        protected AnimationCurve scaleCurve;

        protected float startTime;

        protected bool animating = false;


        protected virtual void Reset()
        {
            animatedTransform = transform;
        }


        /// <summary>
        /// Begin the animation.
        /// </summary>
        public virtual void Animate()
        {
            animating = true;
            startTime = Time.time;
        }


        // Called every frame
        protected virtual void Update()
        {
            if (animating)
            {
                float amount = (Time.time - startTime) / animationTime;
                if (amount > 1)
                {
                    amount = 1;
                    animating = false;
                }

                float scale = scaleCurve.Evaluate(amount);
                transform.localScale = new Vector3(scale, scale, scale);

            }
        }
    }
}

