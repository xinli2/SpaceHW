using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Set up an animator bool parameter that stores the parameter hash so it can be efficiently updated.
    /// </summary>
    [System.Serializable]
    public class AnimatorBoolParameterItem
    {
        [Tooltip("The animator to set the parameter on.")]
        public Animator animator;

        [Tooltip("The parameter name.")]
        public string boolParameterName;

        protected int boolParameterHash;

        /// <summary>
        /// Initialize the hash.
        /// </summary>
        public virtual void Initialize()
        {
            boolParameterHash = Animator.StringToHash(boolParameterName);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="animator">The animator to set the parameter on.</param>
        /// <param name="boolParameterName">The parameter name.</param>
        public AnimatorBoolParameterItem(Animator animator, string boolParameterName)
        {
            this.animator = animator;
            this.boolParameterName = boolParameterName;
            boolParameterHash = Animator.StringToHash(boolParameterName);
        }

        /// <summary>
        /// Set the parameter on the animator.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        public virtual void SetParameter(bool value)
        {
            animator.SetBool(boolParameterHash, value);
        }
    }
}

