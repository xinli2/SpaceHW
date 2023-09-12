using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// A character based on Unity's Character Controller.
    /// </summary>
    public class CharacterStandard : Character
    {

        [Tooltip("This character's character controller.")]
        [SerializeField] 
        protected CharacterController characterController;
        public CharacterController CharacterController { get { return characterController; } }

        [Tooltip("Called when the character's position/rotation is set.")]
        public UnityEvent onCharacterPositionSet;


        /// <summary>
        /// Called to set the position and rotation of the character.
        /// </summary>
        /// <param name="position">The position for the character.</param>
        /// <param name="rotation">The rotation for the character.</param>
        public override void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            base.SetPositionAndRotation(position, rotation);

            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * (characterController.height / 2 - 0.01f), -Vector3.up, out hit, characterController.height, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform != characterController.transform)
                {
                    position = hit.point + Vector3.up * (characterController.height / 2) + Vector3.up * characterController.skinWidth;
                }
            }

            characterController.enabled = false;
            transform.position = position;
            transform.rotation = rotation;
            characterController.enabled = true;

            onCharacterPositionSet.Invoke();
        }
    }
}

