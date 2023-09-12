using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.Utilities.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Controls a health bar that uses Unity's UI.
    /// </summary>
    public class HealthFillBarController : UIFillBarController
    {

        [Tooltip("The health type that is health bar should display.")]
        [SerializeField]
        protected HealthType healthType;
        public HealthType HealthType { get { return healthType; } }

        [Tooltip("The health component that this health bar displays information for.")]
        [SerializeField]
        protected Health health;


        // Called every frame
        protected virtual void Update()
        {
            // Update the health bar
            if (health != null)
            {
                SetFillAmount(health.GetCurrentHealthFractionByType(healthType));
            }
        }
    }
}