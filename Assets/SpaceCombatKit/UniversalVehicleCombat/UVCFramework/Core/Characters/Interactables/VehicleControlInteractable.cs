using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// A character interactable that gives them control of a vehicle.
    /// </summary>
    public class VehicleControlInteractable : CharacterInteractable
    {

        [Tooltip("The vehicle that the charaacter can enter/exit.")]
        [SerializeField]
        protected Vehicle vehicle;


        /// <summary>
        /// Called when a character interacts with this interactable.
        /// </summary>
        /// <param name="character">The character that interacted.</param>
        public override void Interact(Character character)
        {
            if (character.Occupants.Count > 0)
            {
                character.Occupants[0].EnterVehicle(vehicle);
            }
        }
    }
}

