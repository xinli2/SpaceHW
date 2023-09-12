using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Store a reference to an animator and a state, and set whether the character controller should be enabled when the animator is in that state.
    /// </summary>
    [System.Serializable]
    public class AnimatorStateSettings
    {

        [Tooltip("The animator to read the state from")]
        public Animator animator;

        [Tooltip("The name of the state that these settings apply to.")]
        public string stateName;

        [Tooltip("Whether to disable the character controller while the animator is in the specified state.")]
        public bool disableCharacterController;

        // Store the last state to determine if the animator just went into the new state this frame
        [HideInInspector]
        public int lastAnimatorStateFullPathHash = -1;  


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="animator">The animator to read the state from.</param>
        /// <param name="stateName">The name of the state that these settings apply to.</param>
        /// <param name="disableCharacterController">Whether to disable the character controller while the animator is in the specified state.</param>
        public AnimatorStateSettings(Animator animator, string stateName, bool disableCharacterController)
        {
            this.animator = animator;
            this.stateName = stateName;
            this.disableCharacterController = disableCharacterController;
        }
    }
}
