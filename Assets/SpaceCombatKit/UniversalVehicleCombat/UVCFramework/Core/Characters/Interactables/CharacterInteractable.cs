using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Represents an object that a Character can interact with.
    /// </summary>
    public class CharacterInteractable : MonoBehaviour
    {

        [Tooltip("Called when a Character interacts with this interactable.")]
        public UnityEvent onInteract;

        [Tooltip("The prompt that is shown to a character when they can interact with this interactable.")]
        [SerializeField]
        protected string promptText;
        public string PromptText { get { return promptText; } }


        /// <summary>
        /// Called when this interactable is interacted with.
        /// </summary>
        public virtual void Interact()
        {
            onInteract.Invoke();
        }


        /// <summary>
        /// Called when a character interacts with this interactable.
        /// </summary>
        /// <param name="character"></param>
        public virtual void Interact(Character character)
        {
            onInteract.Invoke();
        }
    }
}

