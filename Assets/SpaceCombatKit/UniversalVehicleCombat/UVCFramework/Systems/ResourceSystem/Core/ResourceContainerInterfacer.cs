using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.ResourceSystem
{
    /// <summary>
    /// Interfaces with a resource container that may not be present at the time.
    /// </summary>
    public class ResourceContainerInterfacer : ResourceContainerBase
    {
        [Tooltip("The resource container that this interface points to.")]
        [SerializeField] protected ResourceContainerBase container;
        public virtual ResourceContainerBase Container
        {
            get { return container; }
            set 
            { 
                if (value == null || value.ResourceType == resourceType)
                {
                    container = value;
                }
            }
        }

        [Tooltip("The resource type that this interface can interface with.")]
        [SerializeField] protected ResourceType resourceType;

        public override ResourceType ResourceType
        {
            get { return resourceType; }
        }

        public override float CapacityFloat { get { return container == null ? 0 : container.CapacityFloat; } }
        public override int CapacityInteger { get { return container == null ? 0 : container.CapacityInteger; } }

        public override float ChangeRate
        {
            get { return container == null ? 0 : container.ChangeRate; }
        }

        public override float CurrentAmountFloat { get { return container == null ? 0 : container.CurrentAmountFloat; } }
        public override int CurrentAmountInteger { get { return container == null ? 0 : container.CurrentAmountInteger; } }

        public override bool IsFull
        {
            get { return container == null ? false : container.IsFull; }
        }

        public override bool IsEmpty
        {
            get { return container == null ? false : container.IsEmpty; }
        }


        /// <summary>
        /// Add or remove an amount from the resource container.
        /// </summary>
        /// <param name="amount">The amount to add or remove (positive for add, negative for remove).</param>
        public override void AddRemove(float amount)
        {
            if (container != null)
            {
                container.AddRemove(amount);
            }
        }


        /// <summary>
        /// Add or remove an amount per second (value is multiplied by Time.deltaTime).
        /// </summary>
        /// <param name="amount">The amount to add or remove per second (positive for add, negative for remove).</param>
        public override void AddRemovePerSecond(float amount)
        {
            if (container != null)
            {
                container.AddRemovePerSecond(amount);
            }
        }


        // <summary>
        /// Add or remove a whole amount from the resource container.
        /// </summary>
        /// <param name="amount">The amount to add or remove (positive for add, negative for remove).</param>
        public override void AddRemove(int amount)
        {
            if (container != null)
            {
                container.AddRemove(amount);
            }
        }


        /// <summary>
        /// Check whether the specified amount can be added or removed.
        /// </summary>
        /// <param name="amount">The amount to check if can be added or removed (positive for add check, negative for remove check).</param>
        /// <returns>Whether the specified amount can be added or removed.</returns>
        public override bool CanAddRemove(float amount)
        {
            if (container != null)
            {
                return container.CanAddRemove(amount);
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Check whether the specified whole number amount can be added or removed.
        /// </summary>
        /// <param name="amount">The amount to check if can be added or removed (positive for add check, negative for remove check).</param>
        /// <returns>Whether the specified amount can be added or removed.</returns>
        public override bool CanAddRemove(int amount)
        {
            if (container != null)
            {
                return container.CanAddRemove(amount);
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Fill the container to capacity.
        /// </summary>
        public override void Fill()
        {
            if (container != null)
            {
                container.Fill();
            }
        }


        /// <summary>
        /// Empty the container.
        /// </summary>
        public override void Empty()
        {
            if (container != null)
            {
                container.Empty();
            }
        }


        /// <summary>
        /// Get whether the container has a specified amount or more.
        /// </summary>
        /// <param name="amount">The amount to check for.</param>
        /// <returns>Whether the container has the specified amount or more.</returns>
        public override bool HasAmount(float amount)
        {
            if (container != null)
            {
                return container.HasAmount(amount);
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Get whether the container has a specified whole (integer) amount or more.
        /// </summary>
        /// <param name="amount">The amount to check for.</param>
        /// <returns>Whether the container has the specified amount or more.</returns>
        public override bool HasAmount(int amount)
        {
            if (container != null)
            {
                return container.HasAmount(amount);
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Set the amount in the resource container.
        /// </summary>
        /// <param name="amount">The amount to set.</param>
        public override void SetAmount(float amount)
        {
            if (container != null)
            {
                container.SetAmount(amount);
            }
        }


        /// <summary>
        /// Get whether the container has more than the specified amount.
        /// </summary>
        /// <param name="amount">The amount to check if the container has more than.</param>
        /// <returns>Whether the container has more than the specified amount.</returns>
        public override bool CurrentAmountGreaterThan(float amount)
        {
            if (container != null)
            {
                return container.CurrentAmountGreaterThan(amount);
            }
            else
            {
                return false;
            }
        }


        // <summary>
        /// Get whether the container has less than the specified amount.
        /// </summary>
        /// <param name="amount">The amount to check if the container has less than.</param>
        /// <returns>Whether the container has less than the specified amount.</returns>
        public override bool CurrentAmountLessThan(float amount)
        {
            if (container != null)
            {
                return container.CurrentAmountLessThan(amount);
            }
            else
            {
                return false;
            }
        }
    }
}
