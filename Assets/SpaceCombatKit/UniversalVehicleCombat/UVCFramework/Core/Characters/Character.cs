using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Represents a character that a Game Agent (player or AI) can take control of.
    /// </summary>
    public class Character : Vehicle
    {

        // The interactable that the character can interact with currently.
        protected CharacterInteractable interactable;
        public CharacterInteractable Interactable
        {
            get { return interactable; }
        }

        [Tooltip("Called when the character is teleported to a different location.")]
        public UnityEvent onCharacterTeleported;


        /// <summary>
        /// Get the current height of the character.
        /// </summary>
        public float Height { get { return 1.8f; } }


        /// <summary>
        /// Get the current (world) position of the character's center.
        /// </summary>
        public Vector3 Center { get { return transform.position; } }


        /// <summary>
        /// Called to set the position and rotation of the character.
        /// </summary>
        /// <param name="position">The position for the character.</param>
        /// <param name="rotation">The rotation for the character.</param>
        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation) { }


        /// <summary>
        /// Called to set the position of the character while maintaining its current rotation.
        /// </summary>
        /// <param name="position">The position for the character.</param>
        public virtual void SetPosition(Vector3 position)
        {
            SetPositionAndRotation(position, transform.rotation);
        }


        protected virtual void OnTriggerStay(Collider other)
        {
            CharacterInteractable interactable = other.GetComponent<CharacterInteractable>();
            if (interactable != null)
            {
                this.interactable = interactable;
            }
        }


        protected virtual void OnTriggerExit(Collider other)
        {
            CharacterInteractable interactable = other.GetComponent<CharacterInteractable>();
            if (interactable != null)
            {
                this.interactable = null;
            }
        }


        /// <summary>
        /// Interact with the interactable that this character currently has access to.
        /// </summary>
        public virtual void Interact()
        {
            if (interactable != null)
            {
                interactable.Interact(this);
            }

            interactable = null;
        }
    }
}

