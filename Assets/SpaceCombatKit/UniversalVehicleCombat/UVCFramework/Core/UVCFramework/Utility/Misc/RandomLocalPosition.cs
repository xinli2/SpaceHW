using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Move an object to a random local position relative to its parent.
    /// </summary>
    public class RandomLocalPosition : MonoBehaviour
    {
        [SerializeField] protected Vector2 minMaxX = new Vector2(-1, 1);
        [SerializeField] protected Vector2 minMaxY = new Vector2(-1, 1);
        [SerializeField] protected Vector2 minMaxZ = new Vector2(-1, 1);


        [SerializeField] protected float maxAngleOffsetX = 0.05f;
        [SerializeField] protected float maxAngleOffsetY = 0.05f;


        /// <summary>
        /// Get a new random position.
        /// </summary>
        public virtual void RandomizePosition()
        {
            transform.localPosition = new Vector3(Random.Range(minMaxX.x, minMaxX.y), Random.Range(minMaxY.x, minMaxY.y), Random.Range(minMaxZ.x, minMaxZ.y));

            Vector2 circle = Random.insideUnitCircle;
            circle.x *= maxAngleOffsetX;
            circle.y *= maxAngleOffsetY;

            Vector3 localDirection = new Vector3(circle.x, circle.y, 1);
            transform.localRotation = Quaternion.Euler(localDirection);
        }
    }

}
