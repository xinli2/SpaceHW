using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Represents a volumetric space defined by a Bounds inside which a character is defined as being inside a vehicle.
    /// </summary>
    public class BoundsInteriorVolume : VehicleInteriorVolume
    {

        [Tooltip("The bounds that represents this interior volume.")]
        [SerializeField]
        protected Bounds bounds;


        /// <summary>
        /// Called to check if a world position is within this volume.
        /// </summary>
        /// <param name="worldPosition">The world position.</param>
        /// <returns>Whether the world position is within the volume.</returns>
        public override bool IsInsideVolume(Vector3 worldPosition)
        {
            Vector3 boundsLocalPos = transform.InverseTransformPoint(worldPosition) - bounds.center;

            if (boundsLocalPos.x > bounds.extents.x || boundsLocalPos.x < -bounds.extents.x) return false;
            if (boundsLocalPos.y > bounds.extents.y || boundsLocalPos.y < -bounds.extents.y) return false;
            if (boundsLocalPos.z > bounds.extents.z || boundsLocalPos.z < -bounds.extents.z) return false;

            return true;
        }


        // Editor gizmos
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(bounds.center), transform.rotation, transform.lossyScale);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero, bounds.size);
        }
    }

}
