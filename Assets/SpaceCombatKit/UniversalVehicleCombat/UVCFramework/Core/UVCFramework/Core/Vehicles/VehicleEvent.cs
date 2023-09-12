using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// A unity event that passes a vehicle as a parameter.
    /// </summary>
    [System.Serializable]
    public class VehicleEvent : UnityEvent<Vehicle> { }
}
