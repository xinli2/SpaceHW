using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.ResourceSystem
{
    /// <summary>
    /// Base class for a resource container.
    /// </summary>
    public class ResourceContainerBase : MonoBehaviour
    {
        public virtual ResourceType ResourceType { get { return null; } }

        public virtual float CapacityFloat
        {
            get { return 0; }
            set { }
        }

        public virtual float CurrentAmountFloat
        {
            get { return 0; }
        }

        public virtual int CapacityInteger
        {
            get { return 0; }
        }

        public virtual int CurrentAmountInteger
        {
            get { return 0; }
        }

        public virtual bool IsFull { get { return false; } }

        public virtual bool IsEmpty { get { return false; } }

        public virtual float ChangeRate { get { return 0; } }

        [Header("Events")]

        [Tooltip("Event called when the amount in the resource container changes.")]
        public UnityEvent onChanged;

        [Tooltip("Event called when the resource container becomes empty.")]
        public UnityEvent onEmpty;

        [Tooltip("Event called when the resource container is filled.")]
        public UnityEvent onFilled;


        /// <summary>
        /// Add or remove an amount from the resource container.
        /// </summary>
        /// <param name="amount">The amount to add or remove (positive for add, negative for remove).</param>
        public virtual void AddRemove(float amount) { }


        /// <summary>
        /// Add or remove an amount per second (value is multiplied by Time.deltaTime).
        /// </summary>
        /// <param name="amount">The amount to add or remove per second (positive for add, negative for remove).</param>
        public virtual void AddRemovePerSecond(float amount) { }


        // <summary>
        /// Add or remove a whole amount from the resource container.
        /// </summary>
        /// <param name="amount">The amount to add or remove (positive for add, negative for remove).</param>
        public virtual void AddRemove(int amount) { }


        /// <summary>
        /// Check whether the specified amount can be added or removed.
        /// </summary>
        /// <param name="amount">The amount to check if can be added or removed (positive for add check, negative for remove check).</param>
        /// <returns>Whether the specified amount can be added or removed.</returns>
        public virtual bool CanAddRemove(float amount) { return false; }


        /// <summary>
        /// Check whether the specified whole number amount can be added or removed.
        /// </summary>
        /// <param name="amount">The amount to check if can be added or removed (positive for add check, negative for remove check).</param>
        /// <returns>Whether the specified amount can be added or removed.</returns>
        public virtual bool CanAddRemove(int amount) { return false; }


        /// <summary>
        /// Fill the container to capacity.
        /// </summary>
        public virtual void Fill() { }


        /// <summary>
        /// Empty the container.
        /// </summary>
        public virtual void Empty() { }


        // Called when the container is filled.
        protected virtual void OnFilled()
        {
            onFilled.Invoke();
        }


        // Called when the container is emptied.
        protected virtual void OnEmpty()
        {
            onEmpty.Invoke();
        }


        /// <summary>
        /// Get whether the container has a specified amount or more.
        /// </summary>
        /// <param name="amount">The amount to check for.</param>
        /// <returns>Whether the container has the specified amount or more.</returns>
        public virtual bool HasAmount(float amount)
        {
            return false;
        }


        /// <summary>
        /// Get whether the container has a specified whole (integer) amount or more.
        /// </summary>
        /// <param name="amount">The amount to check for.</param>
        /// <returns>Whether the container has the specified amount or more.</returns>
        public virtual bool HasAmount(int amount)
        {
            return false;
        }


        /// <summary>
        /// Set the amount in the resource container.
        /// </summary>
        /// <param name="amount">The amount to set.</param>
        public virtual void SetAmount(float amount)
        {

        }


        /// <summary>
        /// Get whether the container has more than the specified amount.
        /// </summary>
        /// <param name="amount">The amount to check if the container has more than.</param>
        /// <returns>Whether the container has more than the specified amount.</returns>
        public virtual bool CurrentAmountGreaterThan(float amount)
        {
            return false;
        }


        // <summary>
        /// Get whether the container has less than the specified amount.
        /// </summary>
        /// <param name="amount">The amount to check if the container has less than.</param>
        /// <returns>Whether the container has less than the specified amount.</returns>
        public virtual bool CurrentAmountLessThan(float amount)
        {
            return false;
        }
    }
}
