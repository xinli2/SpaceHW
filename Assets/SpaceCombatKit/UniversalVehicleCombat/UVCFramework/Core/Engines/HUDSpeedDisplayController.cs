using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VSX.Utilities.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Display speed information for a Vehicle Engines 3D component on the HUD
    /// </summary>
    public class HUDSpeedDisplayController : MonoBehaviour 
    {
        [Tooltip("The engines component to display speed information for.")]
        [SerializeField]
        protected VehicleEngines3D engines;

        [SerializeField]
        protected MeshRenderer speedBarRenderer;

        [SerializeField]
        protected UVCText speedText;

        [Tooltip("Speed bar fill image.")]
        [SerializeField]
        protected Image img;


        // Update is called once per frame
        void Update() {
            if (engines != null)
            {
                if (speedBarRenderer != null)
                {
                    speedBarRenderer.material.SetFloat("_FillAmount", engines.Rigidbody.velocity.magnitude / engines.GetCurrentMaxSpeedByAxis(false).z);
                }
                if (img != null) img.fillAmount = engines.Rigidbody.velocity.magnitude / engines.GetCurrentMaxSpeedByAxis(false).z;
                if (speedText != null)
                {
                    speedText.text = ((int)engines.Rigidbody.velocity.magnitude).ToString();
                }
            }
        }
    }
}