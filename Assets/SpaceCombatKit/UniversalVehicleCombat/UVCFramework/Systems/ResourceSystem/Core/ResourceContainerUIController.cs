using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VSX.Utilities.UI;

namespace VSX.ResourceSystem
{
    public class ResourceContainerUIController : MonoBehaviour
    {
        [Tooltip("The resource container that this UI is showing.")]
        [SerializeField]
        protected ResourceContainer resourceContainer;
        public virtual ResourceContainer ResourceContainer
        {
            get { return resourceContainer; }
            set 
            { 
                if (resourceContainer != null)
                {
                    resourceContainer.onChanged.RemoveListener(UpdateUI);
                }

                resourceContainer = value;

                if (resourceContainer != null)
                {
                    resourceContainer.onChanged.AddListener(UpdateUI);
                    UpdateUI();
                }
            }
        }

        [Tooltip("The label for the resource type of the resource container.")]
        [SerializeField]
        protected UVCText resourceLabel;

        [Tooltip("Whether to display the long name of the resource type (if False, will display short name).")]
        [SerializeField]
        protected bool displayLongName = false;

        [Tooltip("The resource icon to display for the resource container.")]
        [SerializeField]
        protected Image resourceIcon;

        [Tooltip("The resource container amount value display.")]
        [SerializeField]
        protected UVCText resourceAmountText;

        [Tooltip("Whether to display the amount in the resource container as a fraction.")]
        [SerializeField]
        protected bool displayFraction = true;

        [Tooltip("The fill bar showing how much is remaining in the resource container.")]
        [SerializeField]
        protected Image fillBarImage;


        protected virtual void Awake()
        {
            ResourceContainer = resourceContainer;
            UpdateUI();
        }


        protected virtual void UpdateUI()
        {
            if (resourceContainer != null)
            {
                if (resourceContainer.ResourceType != null)
                {
                    if (resourceLabel != null) resourceLabel.text = displayLongName ? resourceContainer.ResourceType.LongName : resourceContainer.ResourceType.ShortName;
                    if (resourceIcon != null) resourceIcon.sprite = resourceContainer.ResourceType.Icon;
                }

                if (resourceAmountText != null)
                {
                    resourceAmountText.text = displayFraction ? resourceContainer.CurrentAmountInteger.ToString() + "/" + resourceContainer.CapacityInteger.ToString() : resourceContainer.CurrentAmountInteger.ToString();
                }

                if (fillBarImage != null)
                {
                    fillBarImage.fillAmount = resourceContainer.CurrentAmountFloat / resourceContainer.CapacityFloat;
                }
            }
        }
    }
}
