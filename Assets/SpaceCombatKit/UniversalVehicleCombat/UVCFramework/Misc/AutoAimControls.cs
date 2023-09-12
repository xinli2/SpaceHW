using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Mechs
{
    public class AutoAimControls : VehicleInput
    {

        protected GimballedVehicleAutoAim aimComponent;

        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle))
            {
                return false;
            }

            aimComponent = vehicle.GetComponent<GimballedVehicleAutoAim>();

            return (aimComponent != null);

        }


        protected override void InputUpdate()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (aimComponent.AutoAimEnabled)
                {
                    aimComponent.DisableAutoAim();
                }
                else
                {
                    aimComponent.EnableAutoAim();
                }
            }
        }
    }
}

