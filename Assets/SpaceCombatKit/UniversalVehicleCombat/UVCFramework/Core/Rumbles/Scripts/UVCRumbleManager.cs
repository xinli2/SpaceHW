using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;

namespace VSX.Effects
{
    /// <summary>
    /// A Rumble Manager that includes the option for setting a vehicle reference as the listener.
    /// </summary>
    public class UVCRumbleManager : RumbleManager
    {
        
        [Tooltip("The game agent that is the rumble listener.")]
        [SerializeField]
        protected GameAgent listenerGameAgent;
        public GameAgent ListenerGameAgent { get { return listenerGameAgent; } }


        [Tooltip("Shortcut to set the first player found in the scene as the rumble listener.")]
        [SerializeField]
        protected bool setFirstPlayerAsListener = true;


        protected override void Awake()
        {
            base.Awake();

            if (listener == null && listenerGameAgent == null && setFirstPlayerAsListener)
            {
                GameAgent[] gameAgents = GameObject.FindObjectsOfType<GameAgent>();
                foreach (GameAgent gameAgent in gameAgents)
                {
                    if (gameAgent.IsPlayer)
                    {
                        listenerGameAgent = gameAgent;
                        break;
                    }
                }
            }

            if (listenerGameAgent != null)
            {
                SetListener(listenerGameAgent);
            }
        }


        /// <summary>
        /// Set the listener game agent.
        /// </summary>
        /// <param name="listenerGameAgent">The game agent that is the rumble listener.</param>
        public virtual void SetListener(GameAgent listenerGameAgent)
        {
            if (listenerGameAgent != null)
            {
                listenerGameAgent.onEnteredVehicle.RemoveListener(SetListener);
            }

            this.listenerGameAgent = listenerGameAgent;

            listenerGameAgent.onEnteredVehicle.AddListener(SetListener);
        }

        
        /// <summary>
        /// Set the listener vehicle.
        /// </summary>
        /// <param name="listenerVehicle">The listener vehicle.</param>
        public virtual void SetListener(Vehicle listenerVehicle)
        {
            if (listenerVehicle != null)
            {
                listener = listenerVehicle.transform;
            }
            else
            {
                listener = null;
            }
        }
    }
}
