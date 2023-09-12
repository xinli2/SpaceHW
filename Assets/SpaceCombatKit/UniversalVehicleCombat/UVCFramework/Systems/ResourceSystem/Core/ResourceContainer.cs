using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.ResourceSystem
{
    public class ResourceContainer : ResourceContainerBase
    {

        [Header("Settings")]


        [Tooltip("The resource type that this container holds.")]
        [SerializeField]
        protected ResourceType resourceType;
        public override ResourceType ResourceType
        {
            get { return resourceType; }
        }


        [Tooltip("The capacity of this resource container.")]
        [SerializeField]
        protected float capacity = 100;
        public override float CapacityFloat 
        { 
            get { return capacity; }
            set { capacity = value; currentAmount = Mathf.Min(currentAmount, capacity); }
        }
        public override int CapacityInteger { get { return (int)capacity; } }


        [Tooltip("The internal change rate (per second) of the resources (positive for increase over time, negative for decrease over time).")]
        [SerializeField]
        protected float changeRate = 25;
        public override float ChangeRate
        {
            get { return changeRate; }
        }

        [Tooltip("The starting amount for this resource container.")]
        [SerializeField]
        protected float startAmount = 100;

        protected float currentAmount = 0;
        public override float CurrentAmountFloat { get { return currentAmount; } }
        public override int CurrentAmountInteger { get { return (int)currentAmount; } }

        protected float lastChangeTime = 0;


        [Header("Refill After Empty")]


        [Tooltip("The pause after the container becomes emptied before it is filled (if this feature is enabled).")]
        [SerializeField]
        protected float emptiedPause = 0;

        [Tooltip("Whether to fill the container to capacity after it becomes emptied (after the specified pause).")]
        [SerializeField]
        protected bool fillToCapacityAfterEmptiedPause = true;


        [Header("Empty After Filled")]


        [Tooltip("The pause after the container becomes filled before it is emptied (if this feature is enabled).")]
        [SerializeField]
        protected float filledPause = 0;

        [Tooltip("Whether to empty the container after it becomes full (after the specified pause).")]
        [SerializeField]
        protected bool emptyAfterFilledPause = false;

        protected bool pausing = false;

        protected float pauseStartTime;
        protected float pauseTime;

        public override bool IsFull
        {
            get { return Mathf.Approximately(currentAmount, capacity); }
        }

        public override bool IsEmpty
        {
            get { return Mathf.Approximately(currentAmount, 0); }
        }


        protected virtual void Awake()
        {
            currentAmount = Mathf.Clamp(startAmount, 0, capacity);
        }


        /// <summary>
        /// Add or remove an amount from the resource container.
        /// </summary>
        /// <param name="amount">The amount to add or remove (positive for add, negative for remove).</param>
        public override void AddRemove(float amount)
        {
            float nextValue = currentAmount + amount;

            if (nextValue >= capacity && !Mathf.Approximately(currentAmount, capacity))
            {
                OnFilled();
            }

            if (nextValue <= 0 && !Mathf.Approximately(currentAmount, 0))
            {
                OnEmpty();
            }

            SetAmount(nextValue);

        }


        /// <summary>
        /// Add or remove an amount per second (value is multiplied by Time.deltaTime).
        /// </summary>
        /// <param name="amount">The amount to add or remove per second (positive for add, negative for remove).</param>
        public override void AddRemovePerSecond(float amount)
        {
            AddRemove(amount * Time.deltaTime);
        }


        /// <summary>
        /// Add or remove a whole amount from the resource container.
        /// </summary>
        /// <param name="amount">The amount to add or remove (positive for add, negative for remove).</param>
        public override void AddRemove(int amount)
        {
            AddRemove((float)amount);
        }


        /// <summary>
        /// Check whether the specified amount can be added or removed.
        /// </summary>
        /// <param name="amount">The amount to check if can be added or removed (positive for add check, negative for remove check).</param>
        /// <returns>Whether the specified amount can be added or removed.</returns>
        public override bool CanAddRemove(float amount)
        {

            if (pausing) return false;
            if (amount > 0 && (capacity - currentAmount) < amount) return false;
            if (amount < 0 && (currentAmount + amount) < 0) return false;

            return true;
        }


        /// <summary>
        /// Check whether the specified whole number amount can be added or removed.
        /// </summary>
        /// <param name="amount">The amount to check if can be added or removed (positive for add check, negative for remove check).</param>
        /// <returns>Whether the specified amount can be added or removed.</returns>
        public override bool CanAddRemove(int amount)
        {
            return CanAddRemove((float)amount);
        }


        /// <summary>
        /// Fill the container to capacity.
        /// </summary>
        public override void Fill()
        {
            if (pausing) return;
            SetAmount(capacity);
            OnFilled();
        }


        /// <summary>
        /// Empty the container.
        /// </summary>
        public override void Empty()
        {
            if (pausing) return;
            SetAmount(0);
            OnEmpty();
        }


        // Called when the container is filled.
        protected override void OnFilled()
        {
            base.OnFilled();

            if (filledPause > 0)
            {
                pausing = true;
                pauseStartTime = Time.time;
                pauseTime = filledPause;
            }
        }


        // Called when the container is emptied.
        protected override void OnEmpty()
        {
            base.OnEmpty();

            if (emptiedPause > 0)
            {
                pausing = true;
                pauseStartTime = Time.time;
                pauseTime = emptiedPause;
            }
        }


        /// <summary>
        /// Get whether the container has a specified amount or more.
        /// </summary>
        /// <param name="amount">The amount to check for.</param>
        /// <returns>Whether the container has the specified amount or more.</returns>
        public override bool HasAmount(float amount)
        {
            return (currentAmount >= amount);
        }


        /// <summary>
        /// Set the amount in the resource container.
        /// </summary>
        /// <param name="amount">The amount to set.</param>
        public override void SetAmount(float amount)
        {
            amount = Mathf.Clamp(amount, 0, capacity);

            if (!Mathf.Approximately(amount, currentAmount))
            {
                currentAmount = amount;
                onChanged.Invoke();
            }
        }


        /// <summary>
        /// Get whether the container has a specified whole (integer) amount or more.
        /// </summary>
        /// <param name="amount">The amount to check for.</param>
        /// <returns>Whether the container has the specified amount or more.</returns>
        public override bool HasAmount(int amount)
        {
            return (currentAmount >= amount);
        }


        /// <summary>
        /// Get whether the container has more than the specified amount.
        /// </summary>
        /// <param name="amount">The amount to check if the container has more than.</param>
        /// <returns>Whether the container has more than the specified amount.</returns>
        public override bool CurrentAmountGreaterThan(float value)
        {
            return currentAmount > value;
        }


        // <summary>
        /// Get whether the container has less than the specified amount.
        /// </summary>
        /// <param name="amount">The amount to check if the container has less than.</param>
        /// <returns>Whether the container has less than the specified amount.</returns>
        public override bool CurrentAmountLessThan(float value)
        {
            return currentAmount < value;
        }


        // Called every frame
        protected virtual void Update()
        {
            if (!pausing)
            {
                AddRemove(changeRate * Time.deltaTime);
            }
            else
            {
                // If filled/emptied pause is finished, implement settings
                if (Time.time - pauseStartTime >= pauseTime)
                {
                    pausing = false;

                    if (IsEmpty && fillToCapacityAfterEmptiedPause)
                    {
                        Fill();
                    }
                    else if (IsFull && emptyAfterFilledPause)
                    {
                        Empty();
                    }
                }
            }
        }
    }
}
