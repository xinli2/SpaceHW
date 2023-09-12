using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.Utilities.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Input for the interactions of a Character.
    /// </summary>
    public class CharacterInteractionControls : VehicleInput
    {
        [Tooltip("The game agent that controls the character that this input is for.")]
        [SerializeField]
        protected GameAgent gameAgent;

        protected bool interactionsPaused = false;

        [Tooltip("The interaction UI prompt handle.")]
        [SerializeField]
        protected GameObject prompt;

        [Tooltip("The interaction prompt text.")]
        [SerializeField]
        protected UVCText promptText;

        protected GeneralInputAsset generalInput;

        protected Vehicle vehicle;


        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle)) return false;

            this.vehicle = vehicle;

            return true;
        }


        protected virtual void OnEnable()
        {
            generalInput.Enable();
        }


        protected virtual void OnDisable()
        {
            generalInput.Enable();
        }


        protected override void Awake()
        {
            base.Awake();

            generalInput = new GeneralInputAsset();
            generalInput.GeneralControls.Use.performed += ctx => Interact();
        }


        protected override void InputUpdate()
        {
            if (prompt != null) prompt.SetActive(false);

            if (!interactionsPaused)
            {
                if (gameAgent.Character.Interactable != null)
                {
                    if (promptText != null) promptText.text = gameAgent.Character.Interactable.PromptText;
                    if (prompt != null) prompt.SetActive(true);
                }
            }
        }


        protected virtual void Interact()
        {
            if (!initialized) return;
            if (interactionsPaused) return;

            if (vehicle == gameAgent.Character)
            {
                if (gameAgent.Character.Interactable != null)
                {
                    gameAgent.Character.Interact();
                    PauseInteractions(1);
                }
            }
            else
            {
                if (gameAgent.Character != null)
                {
                    gameAgent.EnterVehicle(gameAgent.Character);
                }
            }
        }


        protected virtual void PauseInteractions(float duration)
        {
            StartCoroutine(PauseInteractionsCoroutine(duration));
        }


        protected IEnumerator PauseInteractionsCoroutine(float duration)
        {
            interactionsPaused = true;
            yield return new WaitForSeconds(duration);
            interactionsPaused = false;
        }
    }
}

