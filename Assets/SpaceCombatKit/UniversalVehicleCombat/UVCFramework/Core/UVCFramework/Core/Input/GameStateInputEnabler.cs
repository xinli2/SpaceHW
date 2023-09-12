using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class GameStateInputEnabler : MonoBehaviour
    {
        [Tooltip("The game states for which the input scripts will be enabled.")]
        [SerializeField]
        protected List<GameState> inputEnabledGameStates = new List<GameState>();
        protected List<GeneralInput> inputs = new List<GeneralInput>();


        protected virtual void Awake()
        {
            // Get all input components in the hierarchy
            inputs = new List<GeneralInput>(GetComponentsInChildren<GeneralInput>());

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.onEnteredGameState.AddListener(OnEnteredGameState);
            }
        }


        protected virtual void OnEnteredGameState(GameState gameState)
        {
            foreach (GeneralInput input in inputs)
            {
                if (inputEnabledGameStates.Contains(gameState))
                {
                    input.EnableInput();
                }
                else
                {
                    input.DisableInput();
                }
            }
        }
    }
}

