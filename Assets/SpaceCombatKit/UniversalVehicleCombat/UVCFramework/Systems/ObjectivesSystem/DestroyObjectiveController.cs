using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;

namespace VSX.ObjectivesSystem
{
    /// <summary>
    /// Manages a single Destroy type objective.
    /// </summary>
    public class DestroyObjectiveController : ObjectiveController
    {

        [Tooltip("The targets that must be destroyed to complete this objective.")]
        [SerializeField]
        protected List<Damageable> targets = new List<Damageable>();
        public List<Damageable> Targets
        {
            get { return targets; }
        }


        protected virtual void Awake()
        {
            foreach(Damageable target in targets)
            {
                target.onDestroyed.AddListener(OnObjectiveChanged);
            }
        }


        /// <summary>
        /// Get how many sub objectives make up this objective.
        /// </summary>
        /// <returns>The number of sub objectives.</returns>
        public override int NumSubObjectives()
        {
            return targets.Count;
        }


        /// <summary>
        /// Get how many sub objectives are completed.
        /// </summary>
        /// <returns>The number of completed sub objectives.</returns>
        public override int NumSubObjectivesCompleted()
        {
            int count = 0;
            foreach(Damageable target in targets)
            {
                if (target.Destroyed) count++;
            }

            return count;
        }


        // Reset this objective.
        protected override void ResetObjective()
        {
            foreach (Damageable target in targets)
            {
                target.Restore();
            }

            base.ResetObjective();
        }


        // Function that is called to check if the objective is complete.
        protected override bool IsCompleted()
        {
            foreach (Damageable target in targets)
            {
                if (!target.Destroyed) return false;
            }

            return true;
        }
    }
}
