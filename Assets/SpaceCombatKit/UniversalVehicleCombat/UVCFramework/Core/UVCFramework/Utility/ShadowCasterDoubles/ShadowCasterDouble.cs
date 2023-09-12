using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Represents an object that has a shadow caster double (used when an object and its shadow caster are not desired to be in the same position).
    /// </summary>
    public class ShadowCasterDouble : MonoBehaviour
    {

        [Tooltip("The shadow caster doubles - objects that cast shadows to represent another object.")]
        [SerializeField]
        protected List<GameObject> shadowCasterObjects = new List<GameObject>();


        /// <summary>
        /// Anchor the shadow caster doubles to a transform.
        /// </summary>
        /// <param name="anchor">The anchor transform</param>
        public virtual void AnchorDouble(Transform anchor)
        {
            foreach (GameObject shadowCasterObject in shadowCasterObjects)
            {
                shadowCasterObject.transform.SetParent(anchor);
                shadowCasterObject.transform.localPosition = Vector3.zero;
                shadowCasterObject.transform.localRotation = Quaternion.identity;
            }
        }


        /// <summary>
        /// Unanchor the shadow caster doubles.
        /// </summary>
        public virtual void UnanchorDouble()
        {
            foreach (GameObject shadowCasterObject in shadowCasterObjects)
            {
                shadowCasterObject.transform.SetParent(transform);
                shadowCasterObject.transform.localPosition = Vector3.zero;
                shadowCasterObject.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
