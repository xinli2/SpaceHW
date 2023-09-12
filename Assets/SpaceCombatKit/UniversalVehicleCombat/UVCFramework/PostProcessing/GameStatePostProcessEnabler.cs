using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Store a reference to a game state and an associated post processing profile
    /// </summary>
    [System.Serializable]
    public class GameStatePostProcessProfile
    {
        [Tooltip("The game states for which this post process profile will be used.")]
        public List<GameState> gameStates = new List<GameState>();

        [Tooltip("The post process profile to use for the specified game states.")]
        public PostProcessProfile profile;
    }

    /// <summary>
    /// Set the post process profile based on game state.
    /// </summary>
    public class GameStatePostProcessEnabler : MonoBehaviour
    {

        [Tooltip("The post process volume to assign the profile to.")]
        [SerializeField]
        protected PostProcessVolume volume;

        [Tooltip("The post process profile to be used by default when no profile has been added specifically for the current game state.")]
        [SerializeField]
        protected PostProcessProfile defaultProfile;

        [Tooltip("The game states and associated profiles to set when they are entered.")]
        [SerializeField]
        protected List<GameStatePostProcessProfile> gameStateProfiles = new List<GameStatePostProcessProfile>();


        protected virtual void Awake()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.onEnteredGameState.AddListener(OnEnteredGameState);
            }

            if (defaultProfile == null) defaultProfile = volume.profile;
        }


        protected virtual void OnEnteredGameState(GameState gameState)
        {
            foreach (GameStatePostProcessProfile gameStateProfile in gameStateProfiles)
            {
                foreach(GameState gs in gameStateProfile.gameStates)
                {
                    if (gs == gameState)
                    {
                        volume.profile = gameStateProfile.profile;
                        return;
                    }
                } 
            }

            volume.profile = defaultProfile;
        }
    }
}
