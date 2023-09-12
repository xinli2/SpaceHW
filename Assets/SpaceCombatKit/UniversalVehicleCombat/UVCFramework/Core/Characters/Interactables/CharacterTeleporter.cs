using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Controller for an interactable that teleports a character to a different location.
    /// </summary>
    public class CharacterTeleporter : CharacterInteractable
    {

        [Tooltip("The transform representing where the character should be teleported.")]
        [SerializeField]
        protected Transform teleportTarget;

        [Tooltip("Whether the character's rotation should be set to the rotation of the teleport target transform.")]
        [SerializeField]
        protected bool controlCharacterRotation = false;


        public override void Interact(Character character)
        {
            if (controlCharacterRotation)
            {
                character.SetPositionAndRotation(teleportTarget.position, teleportTarget.rotation);
            }
            else
            {
                character.SetPosition(teleportTarget.position);
            }
        }
    }
}
