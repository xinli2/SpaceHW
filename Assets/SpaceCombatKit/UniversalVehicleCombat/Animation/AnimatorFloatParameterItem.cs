using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Set up an animator float parameter that stores the parameter hash so it can be efficiently updated.
    /// </summary>
    [System.Serializable]
    public class AnimatorFloatParameterItem
    {

        [Tooltip("The animator to set the parameter on.")]
        public Animator animator;

        [Tooltip("The parameter name.")]
        public string floatParameterName;

        protected int floatParameterHash;

        /// <summary>
        /// Initialize the hash.
        /// </summary>
        public virtual void Initialize()
        {
            floatParameterHash = Animator.StringToHash(floatParameterName);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="animator">The animator to set the parameter on.</param>
        /// <param name="floatParameterName">The parameter name.</param>
        public AnimatorFloatParameterItem(Animator animator, string floatParameterName)
        {
            this.animator = animator;
            this.floatParameterName = floatParameterName;
            floatParameterHash = Animator.StringToHash(floatParameterName);
        }

        /// <summary>
        /// Set the parameter on the animator.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        public virtual void SetParameter(float value)
        {
            animator.SetFloat(floatParameterHash, value);
        }
    }
}
