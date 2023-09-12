using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Represents a volumetric space inside which the character is defined as being inside a vehicle.
    /// </summary>
    public class VehicleInteriorVolume : MonoBehaviour
    {
        [Tooltip("The vehicleinterior that this volume represents.")]
        [SerializeField]
        protected VehicleInterior vehicleInterior;
        public VehicleInterior VehicleInterior
        {
            get { return vehicleInterior; }
        }


        protected virtual void Awake()
        {
            VehicleInteriorRenderer.Instance.Register(this);
        }


        /// <summary>
        /// Called to check if a world position is within this volume.
        /// </summary>
        /// <param name="worldPosition">The world position.</param>
        /// <returns>Whether the world position is within the volume.</returns>
        public virtual bool IsInsideVolume(Vector3 worldPosition)
        {
            return false;
        }
    }
}
