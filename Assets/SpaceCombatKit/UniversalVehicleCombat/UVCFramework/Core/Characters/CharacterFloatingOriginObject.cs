using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.FloatingOriginSystem;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// A floating origin object controller for a character that uses Unity's Character Controller.
    /// </summary>
    public class CharacterFloatingOriginObject : FloatingOriginObject
    {
        [Tooltip("The character that this floating origin object controller is responsible for moving.")]
        [SerializeField]
        protected CharacterStandard character;


        /// <summary>
        /// Called before the floating origin shifts.
        /// </summary>
        public override void OnPreOriginShift()
        {
            character.CharacterController.enabled = false;
            base.OnPreOriginShift();
        }


        /// <summary>
        /// Called after the floating origin shifts.
        /// </summary>
        public override void OnPostOriginShift(Vector3 offset)
        {
            base.OnPostOriginShift(offset);
            character.CharacterController.enabled = true;
        }

    }
}

