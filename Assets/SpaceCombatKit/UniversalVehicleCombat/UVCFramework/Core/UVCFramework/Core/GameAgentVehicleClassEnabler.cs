using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    public class GameAgentVehicleClassEnabler : MonoBehaviour
    {
        public GameAgent gameAgent;
        public List<VehicleClass> compatibleVehicleClasses = new List<VehicleClass>();

        public UnityEvent onCompatibleVehicleClassEntered;
        public UnityEvent onIncompatibleVehicleClassEntered;

        protected virtual void Awake()
        {
            if (gameAgent != null)
            {
                gameAgent.onEnteredVehicle.AddListener(OnVehicleEntered);
                gameAgent.onExitedVehicle.AddListener(OnVehicleExited);
            }
        }


        public virtual void OnVehicleEntered(Vehicle vehicle)
        {
            if (compatibleVehicleClasses.Count == 0 || compatibleVehicleClasses.IndexOf(vehicle.VehicleClass) != -1)
            {
                onCompatibleVehicleClassEntered.Invoke();
            }
            else
            {
                onIncompatibleVehicleClassEntered.Invoke();
            }
        }

        public virtual void OnVehicleExited(Vehicle vehicle)
        {
            onIncompatibleVehicleClassEntered.Invoke();
        }
    }

}
