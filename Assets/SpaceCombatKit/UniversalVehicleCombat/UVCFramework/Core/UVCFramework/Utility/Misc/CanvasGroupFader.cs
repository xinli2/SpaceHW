using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Fade a canvas group according to an animation curve.
    /// </summary>
    public class CanvasGroupFader : MonoBehaviour
    {
        [SerializeField]
        protected bool loop;

        [SerializeField]
        protected bool startOnEnable;

        [SerializeField]
        protected AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [SerializeField]
        protected float animationLength = 3;
        public float AnimationLength 
        { 
            get { return animationLength; }
            set { animationLength = value; }
        }

        protected float animationStartTime;
        protected bool animating;
        public bool Animating { get { return animating; } }

        [SerializeField]
        protected float startAlpha = 0;

        [SerializeField]
        protected CanvasGroup canvasGroup;

        public UnityEvent onAnimationStarted;
        public UnityEvent onAnimationStopped;

        protected float currentDelay;


        // Called when this component is first added to a gameobject or reset in the inspector
        protected virtual void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }


        protected virtual void Awake()
        {
            SetAlpha(startAlpha);
        }


        protected virtual void OnEnable()
        {
            if (startOnEnable)
            {
                StartAnimation();
            }
        }


        /// <summary>
        /// Start animating the canvas group.
        /// </summary>
        public virtual void StartAnimation()
        {
            currentDelay = 0;
            animating = true;
            animationStartTime = Time.time;
            onAnimationStarted.Invoke();
        }


        /// <summary>
        /// Stop animating the canvas group.
        /// </summary>
        public virtual void StopAnimation()
        {
            animating = false;
            onAnimationStopped.Invoke();
        }


        public virtual void StartAnimationDelayed(float delay)
        {
            currentDelay = delay;
            animating = true;
            animationStartTime = Time.time;
            onAnimationStarted.Invoke();
        }


        public virtual void SetAlpha(float alpha)
        {
            canvasGroup.alpha = alpha;
        }


        // Called every frame
        protected virtual void Update()
        {
            if (animating)
            {
                float amount = (Time.time - animationStartTime - currentDelay) / animationLength;

                // If finished, finish animating
                if (amount >= 1)
                {
                    SetAlpha(alphaCurve.Evaluate(1));

                    // If looping, start again
                    if (loop)
                    {
                        StartAnimation();
                    }
                    else
                    {
                        StopAnimation();
                    }
                }
                // If still animating, update the alpha
                else if (amount > 0)
                {
                    float curveAmount = alphaCurve.Evaluate(amount);
                    SetAlpha(curveAmount);
                }
            }
        }
    }
}

