using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.Utilities.UI;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Base class for displaying a stat about a loadout item.
    /// </summary>
    public class StatsInstance : MonoBehaviour
    {
        [Tooltip("The loadout stat label.")]
        [SerializeField] protected UVCText labelText;

        [Tooltip("The loadout stat value text.")]
        [SerializeField] protected UVCText valueText;

        [Tooltip("The loadout stat comparison bar.")]
        [SerializeField] protected UIFillBarController fillBar;

        /// <summary>
        /// Set the stats info content.
        /// </summary>
        /// <param name="label">The stat label.</param>
        /// <param name="value">The stat value.</param>
        /// <param name="barValue">The value to display for a stat fill bar.</param>
        public void Set(string label, string value, float barValue)
        {
            labelText.gameObject.SetActive(true);
            labelText.text = label;

            valueText.gameObject.SetActive(true);
            valueText.text = value;

            fillBar.gameObject.SetActive(true);
            fillBar.SetFillAmount(barValue);
        }


        /// <summary>
        /// Set the stats info content.
        /// </summary>
        /// <param name="label">The stat label.</param>
        /// <param name="value">The stat value.</param>
        public void Set(string label, string value)
        {
            labelText.gameObject.SetActive(true);
            labelText.text = label;

            valueText.gameObject.SetActive(true);
            valueText.text = value;

            fillBar.gameObject.SetActive(false);
        }


        /// <summary>
        /// Set the stats info content.
        /// </summary>
        /// <param name="label">The stat label.</param>
        /// <param name="barValue">The value to display for a stat fill bar.</param>
        public void Set(string label, float barValue)
        {
            labelText.gameObject.SetActive(true);
            labelText.text = label;

            valueText.gameObject.SetActive(false);

            fillBar.gameObject.SetActive(true);
            fillBar.SetFillAmount(barValue);
        }
    }
}
