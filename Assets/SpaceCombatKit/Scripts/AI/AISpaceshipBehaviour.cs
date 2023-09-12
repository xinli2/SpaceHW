using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class AISpaceshipBehaviour : AIVehicleBehaviour
    {

        [SerializeField]
        protected ShipPIDController shipPIDController;

        [SerializeField]
        protected Vector3 maxRotationAngles = new Vector3(360, 360, 360);

        protected VehicleEngines3D engines;


        protected virtual void SteerToward(Vector3 steeringTarget) 
        {
            Maneuvring.TurnToward(vehicle.transform, steeringTarget, maxRotationAngles, shipPIDController.steeringPIDController);
            engines.SetSteeringInputs(shipPIDController.GetSteeringControlValues());
        }

        protected virtual void MoveToward(Vector3 moveTarget) 
        {
            Maneuvring.TranslateToward(engines.Rigidbody, moveTarget, shipPIDController.movementPIDController);
            engines.SetMovementInputs(shipPIDController.GetMovementControlValues());
        }
    }
}