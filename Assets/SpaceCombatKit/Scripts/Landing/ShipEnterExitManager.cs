using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class ShipEnterExitManager : VehicleEnterExitManager
    {

        [Header("Ship Enter/Exit Settings")]

        [Tooltip("The enter/exit manager for this ship.")]
        [SerializeField]
        protected ShipLander shipLander;

        [SerializeField]
        protected bool launchShipOnChildEnter = true;

        [SerializeField]
        protected bool exitOnlyWhenLanded = true;

        /// <summary>
        /// Whether the child vehicle that has entered this vehicle can exit.
        /// </summary>
        /// <returns>Whether the child vehicle that has entered this vehicle can exit.</returns>
        public override bool CanExitToChild()
        {
            if (!base.CanExitToChild()) return false;

            // Only allow exiting if the ship has landed
            if (exitOnlyWhenLanded && shipLander != null && shipLander.CurrentState != ShipLander.ShipLanderState.Landed)
            {
                return false;
            }

            return true;
        }

        public override void OnChildEntered(VehicleEnterExitManager child)
        {
            base.OnChildEntered(child);

            if (shipLander != null && child != null && launchShipOnChildEnter)
            {
                shipLander.Launch();
            }
        }
    }
}