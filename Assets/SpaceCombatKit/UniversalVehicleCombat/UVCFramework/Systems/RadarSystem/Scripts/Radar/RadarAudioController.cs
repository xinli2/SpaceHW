using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{
    public class RadarAudioController : MonoBehaviour
    {
        [Tooltip("All the trackers to play audio for.")]
        [SerializeField]
        protected List<Tracker> trackers = new List<Tracker>();

        [Tooltip("Audio clip played when a new non hostile contact is detected.")]
        [SerializeField]
        protected AudioSource newNonHostileContactAudio;

        protected float lastNonHostileContactAudioTime;

        [Tooltip("Audio clip played when a new hostile contact is detected.")]
        [SerializeField]
        protected AudioSource newHostileContactAudio;

        protected float lastHostileContactAudioTime;

        [Tooltip("The minimum interval between playing new contact audio to avoid audio glitches.")]
        [SerializeField]
        protected float minNewContactAudioInterval = 0.1f;

        [Tooltip("The delay between when a new contact is detected and when the audio plays.")]
        [SerializeField]
        protected float newContactAudioDelay = 0;

        

        [Header("Hostile Alarm")]

        [Tooltip("The teams that are hostile to the vehicle with the trackers that the audio is playing for.")]
        [SerializeField]
        protected List<Team> hostileTeams = new List<Team>();

        [Tooltip("Alarm sounded the first time a hostile is detected after a period with no hostiles detected.")]
        [SerializeField]
        protected AudioSource hostileTeamDetectedAudio;

        [Tooltip("The amount of time to wait after starting the scene before the hostile alarm can be played. Prevents unwanted audio at scene start.")]
        [SerializeField]
        protected float hostileAlarmStartingSilence = 1f;

        [Tooltip("The time between when the first hostile is detected and the alarm is sounded.")]
        [SerializeField]
        protected float hostileAlarmDelay = 0.25f;

        protected int numHostilesTracked = 0;


        protected virtual void Awake()
        {
            for(int i = 0; i < trackers.Count; ++i)
            {
                trackers[i].onStartedTracking.AddListener(OnStartedTrackingTarget);
                trackers[i].onStoppedTracking.AddListener(OnStoppedTrackingTarget);
            }
        }

        /// <summary>
        /// Called when a new target is tracked.
        /// </summary>
        /// <param name="newTarget">The new target.</param>
        public virtual void OnStartedTrackingTarget(Trackable target)
        {
            if (!enabled) return;

            if (target == null) return;

            if (hostileTeams.IndexOf(target.Team) != -1)
            {

                // Play new hostile contact audio
                if (newHostileContactAudio != null && newHostileContactAudio.gameObject.activeInHierarchy && Time.time - lastHostileContactAudioTime > minNewContactAudioInterval)
                {
                    newHostileContactAudio.PlayDelayed(newContactAudioDelay);
                    lastHostileContactAudioTime = Time.time;
                }

                // If this is the first hostile detected by the radar, raise the alarm
                if (numHostilesTracked == 0)
                {
                    if (Time.timeSinceLevelLoad < hostileAlarmStartingSilence) return;

                    if (hostileTeamDetectedAudio != null && hostileTeamDetectedAudio.gameObject.activeInHierarchy)
                    {
                        hostileTeamDetectedAudio.PlayDelayed(hostileAlarmDelay);
                    }
                }

                numHostilesTracked += 1;
            }
            else
            {
                // Play new non-hostile contact audio
                if (newNonHostileContactAudio != null && newNonHostileContactAudio.gameObject.activeInHierarchy && Time.time - lastNonHostileContactAudioTime > minNewContactAudioInterval)
                {
                    newNonHostileContactAudio.PlayDelayed(newContactAudioDelay);
                    lastNonHostileContactAudioTime = Time.time;
                }
            }
        }


        /// <summary>
        /// Called when a target stops being tracked.
        /// </summary>
        /// <param name="newTarget">The untracked target</param>
        public virtual void OnStoppedTrackingTarget(Trackable target)
        {
            if (!enabled) return;

            if (target == null) return;

            // If the untracked target is hostile, reduce the count of hostiles being tracked
            if (target.Team != null && hostileTeams.Contains(target.Team))
            {
                numHostilesTracked -= 1;
            }
        }
    }
}
