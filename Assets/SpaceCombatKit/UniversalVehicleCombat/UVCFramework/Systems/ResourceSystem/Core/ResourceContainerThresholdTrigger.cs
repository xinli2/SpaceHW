using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VSX.ResourceSystem
{
    /// <summary>
    /// Trigger an event when an amount threshold is crossed in a resource container.
    /// </summary>
    public class ResourceContainerThresholdTrigger : MonoBehaviour
    {

        [Tooltip("The resource container this trigger refers to.")]
        [SerializeField] protected ResourceContainerBase container;

        [Tooltip("The amount threshold value.")]
        [SerializeField] protected float threshold = 100;
        protected float lastLevel;  // Store the last value so can check if the threshold was crossed this frame.

        [Tooltip("Trigger when the amount becomes greater than the threshold value (if False, will trigger when becomes less than).")]
        [SerializeField]
        protected bool greaterThan = true;

        [Tooltip("The delay before the event is triggered.")]
        [SerializeField] protected float eventDelay = 5;

        [Tooltip("Fill bar showing the amount of the threshold value that the resource container contains.")]
        [SerializeField] protected Image thresholdFillBar;

        [Tooltip("Whether to reset the trigger when this component is enabled.")]
        [SerializeField]
        protected bool resetTriggerOnEnable = true;
        
        protected float timerStartTime;
        protected bool timerStarted = false;

        [Tooltip("Event called when the threshold is crossed.")]
        public UnityEvent onThresholdCrossed;

        [Tooltip("Event called when the threshold is returned (crossed in the reverse direction).")]
        public UnityEvent onThresholdReturned;

        [Tooltip("Event called when this trigger is triggered.")]
        public UnityEvent onTriggered;

        protected bool triggered = false;


        protected virtual void OnEnable()
        {
            if (resetTriggerOnEnable)
            {
                triggered = false;
                lastLevel = 0;
                timerStarted = false;
            }
        }
        

        protected virtual void CheckThreshold()
        {

            float currentAmount = container.CurrentAmountFloat;     

            if (greaterThan)
            {
                if (currentAmount > threshold && lastLevel <= threshold)
                {
                    OnThresholdCrossed();
                }
                else if (currentAmount < threshold && lastLevel >= threshold)
                {
                    OnThresholdReturned();
                }
            }
            else
            {
                if (currentAmount < threshold && lastLevel >= threshold)
                {
                    OnThresholdCrossed();
                }
                else if (currentAmount > threshold && lastLevel <= threshold)
                {
                    OnThresholdReturned();
                }
            }        

            lastLevel = currentAmount;

            if (thresholdFillBar != null)
            {
                thresholdFillBar.fillAmount = Mathf.Clamp(currentAmount / threshold, 0, 1);
            }
        }


        protected virtual void OnThresholdCrossed()
        {
            if (!timerStarted)
            {
                timerStartTime = Time.time;
                timerStarted = true;
                onThresholdCrossed.Invoke();
            }
        }


        protected virtual void OnThresholdReturned()
        {
            if (timerStarted)
            {
                timerStarted = false;
                onThresholdReturned.Invoke();
            }
        }


        protected virtual void Update()
        {
            if (!triggered && timerStarted)
            {
                if (Time.time - timerStartTime > eventDelay)
                {
                    onTriggered.Invoke();
                    triggered = true;
                }
            }
        }

        protected virtual void LateUpdate()
        {
            if (!triggered) CheckThreshold();    // Must be done in lateupdate or threshold will be crossed back and forth in multiple successive frames
        }
    }
}
