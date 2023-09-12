using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.Utilities
{
    /// <summary>
    /// Receives events from an animator set up for state animations. This component may be used as a dummy component to supress Unity's error that is triggered when
    /// an animation event has no component receiver.
    /// </summary>
    public class StateAnimatorEventReceiver : MonoBehaviour
    {
        /// <summary>
        /// Called via animation event when a state animation finishes.
        /// </summary>
        public virtual void OnStateAnimationFinished() { }
    }
}
