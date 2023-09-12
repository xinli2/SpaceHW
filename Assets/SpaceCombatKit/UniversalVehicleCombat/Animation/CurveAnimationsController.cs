using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    // The type of animation that a specific curve drives.
    public enum CurveAnimationType
    {
        Position,
        Rotation
    }

    // A single instance of a curve-driven animation
    [System.Serializable]
    public class CurveAnimation
    {

        public bool enabled = true;

        public Transform animatedTransform;

        public CurveAnimationType type;

        public float frequency = 1;

        public float offset;

        public bool relativeToCurrent;

        public Vector3 startRotation;
        public Vector3 endRotation;

        public Vector3 startPosition;
        public Vector3 endPosition;

        [HideInInspector]
        public Vector3 originalPosition;

        [HideInInspector]
        public Quaternion originalRotation;

        public AnimationCurve curve;

        /// <summary>
        /// Reset the animated transform to its original position and rotation
        /// </summary>
        public void Reset()
        {
            animatedTransform.localPosition = originalPosition;
            animatedTransform.localRotation = originalRotation;
        }
    }

    /// <summary>
    /// Animate the position and rotation of a transform using animation curves
    /// </summary>
    public class CurveAnimationsController : MonoBehaviour
    {

        [SerializeField]
        protected List<CurveAnimation> curveAnimations = new List<CurveAnimation>();

        // Shot Animation

        [SerializeField]
        protected float animationShotTime = 1;
        protected float animationShotStartTime;
        protected bool animating = false;

        [SerializeField]
        protected bool animateOnUpdate;

        public UnityEvent shotFinishedCallback;


        protected virtual void Start()
        {
            // Store the original local position and rotation
            for (int i = 0; i < curveAnimations.Count; ++i)
            {
                curveAnimations[i].originalPosition = curveAnimations[i].animatedTransform.localPosition;
                curveAnimations[i].originalRotation = curveAnimations[i].animatedTransform.localRotation;
            }
        }

        /// <summary>
        /// Play one shot of the animation
        /// </summary>
        public virtual void PlayOneShot()
        {
            animationShotStartTime = Time.time;
            animating = true;
        }

        /// <summary>
        /// Animate a single frame at a specified scale. 
        /// </summary>
        /// <param name="animationStrength">The animation strength (0-1).</param>
        public virtual void Animate(float animationStrength)
        {
            Animate(animationStrength, Time.time);
        }

        public virtual void Animate(float animationStrength, float position)
        {
            if (!gameObject.activeInHierarchy) return;

            // Reset the animations
            for (int i = 0; i < curveAnimations.Count; ++i)
            {
                curveAnimations[i].Reset();
            }

            // Position the animation relative to the specified position
            for (int i = 0; i < curveAnimations.Count; ++i)
            {
                if (!curveAnimations[i].enabled) continue;
                float period = 1 / curveAnimations[i].frequency;
                float curvePosition = (position % period) / period;

                switch (curveAnimations[i].type)
                {
                    case CurveAnimationType.Position:

                        Vector3 start, end;
                        if (curveAnimations[i].relativeToCurrent)
                        {
                            start = curveAnimations[i].startPosition + curveAnimations[i].animatedTransform.localPosition;
                            end = curveAnimations[i].endPosition + curveAnimations[i].animatedTransform.localPosition;
                        }
                        else
                        {
                            start = curveAnimations[i].startPosition;
                            end = curveAnimations[i].endPosition;
                        }

                        float curveValue = curveAnimations[i].curve.Evaluate((curvePosition + curveAnimations[i].offset) % 1);
                        Vector3 pos = curveValue * end + (1 - curveValue) * start;
                        curveAnimations[i].animatedTransform.localPosition = (animationStrength * pos + (1 - animationStrength) * curveAnimations[i].originalPosition);
                        break;

                    case CurveAnimationType.Rotation:

                        Quaternion startRot, endRot;
                        if (curveAnimations[i].relativeToCurrent)
                        {
                            startRot = Quaternion.Euler(curveAnimations[i].startRotation) * curveAnimations[i].animatedTransform.localRotation;
                            endRot = Quaternion.Euler(curveAnimations[i].endRotation) * curveAnimations[i].animatedTransform.localRotation;
                        }
                        else
                        {
                            startRot = Quaternion.Euler(curveAnimations[i].startRotation);
                            endRot = Quaternion.Euler(curveAnimations[i].endRotation);
                        }

                        float curveVal = curveAnimations[i].curve.Evaluate((curvePosition + curveAnimations[i].offset) % 1);
                        Quaternion rot = Quaternion.Slerp(startRot, endRot, curveVal);
                        curveAnimations[i].animatedTransform.localRotation = Quaternion.Slerp(curveAnimations[i].originalRotation, rot, animationStrength);
                        break;

                }
            }
        }

        public void ResetAnimations()
        {
            for(int i = 0; i < curveAnimations.Count; ++i)
            {
                curveAnimations[i].Reset();
            }
        }

        // Called every frame
        private void Update()
        {
            // Animate
            if (animating)
            {
                float timePosition = Time.time - animationShotStartTime;
                if (timePosition > 1)
                {
                    Animate(1, animationShotTime);
                    animating = false;
                }
                else
                {
                    Animate(1, timePosition);
                }
            }
            else
            {
                if (animateOnUpdate)
                {
                    Animate(1, (Time.time % animationShotTime) / animationShotTime);
                }
            }
        }
    }
}
