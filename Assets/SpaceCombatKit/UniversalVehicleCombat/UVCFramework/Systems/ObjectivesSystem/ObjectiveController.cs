using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.ObjectivesSystem
{
    /// <summary>
    /// The base class for managing a single objective.
    /// </summary>
    public class ObjectiveController : MonoBehaviour
    {

        [Tooltip("Event called when something about the objective changes.")]
        public UnityEvent onChanged;

        [Tooltip("Event called when the objective is completed.")]
        public UnityEvent onCompleted;

        [Tooltip("Event called when the objective is reset.")]
        public UnityEvent onReset;

        protected bool completed = false;
        public virtual bool Completed { get { return completed; } }


        /// <summary>
        /// Get how many sub objectives make up this objective.
        /// </summary>
        /// <returns>The number of sub objectives.</returns>
        public virtual int NumSubObjectives()
        {
            return 1;
        }


        /// <summary>
        /// Get how many sub objectives are completed.
        /// </summary>
        /// <returns>The number of completed sub objectives.</returns>
        public virtual int NumSubObjectivesCompleted()
        {
            return 0;
        }


        // Set the objective completed.
        protected virtual void SetObjectiveCompleted()
        {
            completed = true;
            onCompleted.Invoke();
        }


        // Reset the objective.
        protected virtual void ResetObjective()
        {
            completed = false;
            onReset.Invoke();
        }


        /// <summary>
        /// Check if the objective is completed.
        /// </summary>
        public virtual void CheckIsCompleted()
        {
            if (IsCompleted())
            {
                SetObjectiveCompleted();
            }
        }


        // Function that is called to check if the objective is complete.
        protected virtual bool IsCompleted()
        {
            return false;
        }


        // Called when something about the objective changes.
        protected virtual void OnObjectiveChanged()
        {
            CheckIsCompleted();
            onChanged.Invoke();
        }
    }
}
