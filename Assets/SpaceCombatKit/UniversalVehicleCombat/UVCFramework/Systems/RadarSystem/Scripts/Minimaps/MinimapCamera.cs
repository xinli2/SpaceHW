using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Designates a camera that renders a minimap.
    /// </summary>
    public class MinimapCamera : MonoBehaviour
    {
        [SerializeField] protected Camera m_Camera;
        public Camera Camera
        {
            get { return m_Camera; }
            set { m_Camera = value; }
        }

        [SerializeField] protected Transform mapOrigin;
        public Transform MapOrigin
        {
            get { return mapOrigin; }
            set { mapOrigin = value; }
        }


        protected virtual void Reset()
        {
            m_Camera = GetComponentInChildren<Camera>();
            mapOrigin = transform;
        }

    }
}

