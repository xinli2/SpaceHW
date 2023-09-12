using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.ResourceSystem
{
    /// <summary>
    /// Represents a resource handler (something that adds or removes resources from a container).
    /// </summary>
    [System.Serializable]
    public class ResourceHandler
    {
        [Tooltip("The resource container to add/remove resources from.")]
        public ResourceContainerBase resourceContainer;

        [Tooltip("The amount to add or remove (positive for add, negative for remove).")]
        public float unitResourceChange;

        [Tooltip("Whether the resource change is per second (multiplied by Time.deltaTime).")]
        public bool perSecond = false;

        [Tooltip("Whether to set the resource container empty if insufficient resources are available (enables the container to become fully empty).")]
        public bool setEmptyWhenInsufficient = true;


        /// <summary>
        /// Check if the resource container can add or remove the specified amount.
        /// </summary>
        /// <param name="numResourceChanges">How many resource changes to check for (e.g. if there are multiple weapon units making up the weapon).</param>
        /// <returns>Whether the resource container can add or remove the specified amount.</returns>
        public bool Ready(int numResourceChanges = 1)
        {
            if (setEmptyWhenInsufficient && unitResourceChange < 0)
            {
                if (!resourceContainer.HasAmount(numResourceChanges * Mathf.Abs(unitResourceChange) * (perSecond ? Time.deltaTime : 1))) resourceContainer.Empty();
            }

            if (!resourceContainer.CanAddRemove(numResourceChanges * unitResourceChange * (perSecond ? Time.deltaTime : 1))) return false;

            return true;
        }


        /// <summary>
        /// Implement the resource changes.
        /// </summary>
        /// <param name="numResourceChanges">The number of resource changes to implement.</param>
        public virtual void Implement(int numResourceChanges = 1)
        {
            resourceContainer.AddRemove(numResourceChanges * unitResourceChange * (perSecond ? Time.deltaTime : 1));
        }
    }
}
