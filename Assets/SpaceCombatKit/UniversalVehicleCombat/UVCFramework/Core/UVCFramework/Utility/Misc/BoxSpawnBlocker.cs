using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Represents a box shaped volume that prevents object spawning inside it.
    /// </summary>
    public class BoxSpawnBlocker : MassObjectSpawnBlocker
    {

        [Tooltip("The dimensions of the box spawn blocker.")]
        [SerializeField]
        protected Vector3 dimensions = new Vector3(100, 100, 100);


        /// <summary>
        /// Whether a specified position is inside the box (blocked from spawning objects);
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <returns>Whether the position is inside the box.</returns>
        public override bool IsBlocked(Vector3 position)
        {
            Vector3 localPos = transform.InverseTransformPoint(position);

            if (Mathf.Abs(localPos.x) > dimensions.x / 2f) return false;
            if (Mathf.Abs(localPos.y) > dimensions.y / 2f) return false;
            if (Mathf.Abs(localPos.z) > dimensions.z / 2f) return false;

            return true;
        }


        protected override void OnDrawGizmosSelected()
        {
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(transform.position, dimensions);
            Gizmos.matrix = originalMatrix;
        }
    }
}

