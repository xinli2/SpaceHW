using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Manages a single lead target box on a target box on the HUD.
    /// </summary>
    public class HUDTargetBox_LeadTargetBoxController : MonoBehaviour
    {

        [SerializeField]
        protected Image box;
        public Image Box { get { return box; } }

        [SerializeField]
        protected Image line;
        public Image Line { get { return line; } }

        [Tooltip("This is the value that is multiplied by the ratio of the line width to the box width, to determine the alpha. Fades out the lead target box near the center.")]
        [SerializeField]
        protected float lineLengthToBoxWidthAlphaCoefficient = 1f;


        /// <summary>
        /// Update the lead target box
        /// </summary>
        public void UpdateLeadTargetBox()
        {
            
            // Set the line position
            line.rectTransform.localPosition = 0.5f * box.rectTransform.localPosition;

            // Set the rotation
            if ((box.rectTransform.position - box.rectTransform.parent.position).magnitude < 0.0001f)
            {
                line.rectTransform.rotation = Quaternion.identity;
            }
            else
            {
                line.rectTransform.rotation = Quaternion.LookRotation(box.rectTransform.position - box.rectTransform.parent.position, Vector3.up);
            }
            line.transform.Rotate(Vector3.up, 90, UnityEngine.Space.Self);

            // Set the line size
            Vector2 size = line.rectTransform.sizeDelta;
            size.x = 2 * Vector3.Magnitude(line.rectTransform.localPosition);

            line.rectTransform.sizeDelta = size;

            // Fade the lead target box/line when near the center
            if (box.rectTransform.sizeDelta.x > 0.0001f)
            {
                Color c = line.color;
                c.a = Mathf.Clamp(lineLengthToBoxWidthAlphaCoefficient * (line.rectTransform.sizeDelta.x / box.rectTransform.sizeDelta.x), 0, 1);
                line.color = c;

                c = box.color;
                c.a = Mathf.Clamp(lineLengthToBoxWidthAlphaCoefficient * (line.rectTransform.sizeDelta.x / box.rectTransform.sizeDelta.x), 0, 1);
                box.color = c;
            }
        }

        /// <summary>
        /// Activate the lead target box.
        /// </summary>
        public void Activate()
        {
            box.gameObject.SetActive(true);
            line.gameObject.SetActive(true);
        }

        /// <summary>
        /// Deactivate the lead target box.
        /// </summary>
        public void Deactivate()
        {
            box.gameObject.SetActive(false);
            line.gameObject.SetActive(false);
        }
    }
}