using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Enable audio sources to ignore listener pause and continue playing when timescale is zero, useful for menu sounds.
    /// </summary>
    public class IgnoreListenerPause : MonoBehaviour
    {
        [Tooltip("The audio sources that should continue playing when the timescale = 0.")]
        [SerializeField]
        protected List<AudioSource> audioSources = new List<AudioSource>();

        protected virtual void Reset()
        {
            audioSources = new List<AudioSource>(GetComponentsInChildren<AudioSource>());
        }

        protected virtual void Awake()
        {
            foreach (AudioSource audioSource in audioSources)
            {
                audioSource.ignoreListenerPause = true;
            }
        }
    }
}
