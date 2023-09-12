using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class LightAnimator : MonoBehaviour
    {
        public Light m_Light;

        public float maxIntensity = 1;

        protected float intensityMultiplier = 1;
        public float IntensityMultiplier
        {
            get { return intensityMultiplier; }
            set { intensityMultiplier = value; }
        }


        [Header("One Shot Animation")]

        public float oneShotAnimationLength = 1;

        public AnimationCurve oneShotAnimationIntensityCurve = AnimationCurve.Linear(0, 1, 1, 0);

        protected float oneShotAnimationStartTime;
        protected bool oneShotAnimationStarted = false;

        public bool playOneShotAnimationOnEnable = true;

        protected float oneShotAnimationValue = 1;


        protected virtual void Reset()
        {
            m_Light = GetComponentInChildren<Light>();
        }


        protected virtual void Awake()
        {
            m_Light.intensity = 0;
        }


        protected virtual void OnEnable()
        {
            if (playOneShotAnimationOnEnable)
            {
                PlayOneShotAnimation();
            }
        }


        public virtual void PlayOneShotAnimation()
        {
            oneShotAnimationStartTime = Time.time;
            oneShotAnimationStarted = true;
        }


        void UpdateLightIntensity()
        {
            m_Light.intensity = maxIntensity * oneShotAnimationValue * intensityMultiplier;
        }


        private void Update()
        {
            if (oneShotAnimationStarted)
            {
                float amount = (Time.time - oneShotAnimationStartTime) / oneShotAnimationLength;
                if (amount > 1)
                {
                    oneShotAnimationValue = oneShotAnimationIntensityCurve.Evaluate(1);
                    oneShotAnimationStarted = false;
                }
                else
                {
                    oneShotAnimationValue = oneShotAnimationIntensityCurve.Evaluate(amount);
                }
            }

            UpdateLightIntensity();
        }

    }
}

