using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Stores data for an audio source that is modulated for both pitch and volume for a single level input.
    /// </summary>
    [System.Serializable]
    public class ModulatedAudio
    {
        [Tooltip("The audio source to modulate.")]
        [SerializeField]
        protected AudioSource audioSource;

        [Tooltip("Whether to modulate volume")]
        [SerializeField]
        protected bool modulateVolume = true;

        [Tooltip("The curve that describes the volume for a level input.")]
        [SerializeField]
        protected AnimationCurve volumeCurve = AnimationCurve.Linear(0, 1, 1, 1);

        [Tooltip("Whether to modulate pitch.")]
        [SerializeField]
        protected bool modulatePitch = true;

        [Tooltip("The curve that describes the pitch for a level input.")]
        [SerializeField]
        protected AnimationCurve pitchCurve = AnimationCurve.Linear(0, 1, 1, 1);

        /// <summary>
        /// Set the level of the audio.
        /// </summary>
        /// <param name="level">The level value.</param>
        public virtual void SetLevel(float level)
        {
            audioSource.volume = volumeCurve.Evaluate(level);
            audioSource.pitch = pitchCurve.Evaluate(level);
        }
    }
}
