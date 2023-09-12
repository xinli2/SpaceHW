using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VSX.Utilities.UI;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Base class to manage the display of info for a loadout item.
    /// </summary>
    public class LoadoutItemInfoController : MonoBehaviour
    {
        [Tooltip("The UI handle to activate (to show the info UI) or deactivate (to hide the info UI).")]
        [SerializeField]
        protected GameObject UIHandle;

        [Tooltip("The loadout manager to display info for.")]
        [SerializeField]
        protected LoadoutManager loadoutManager;

        protected List<LoadoutItemInfoOverrideController> overrideControllers = new List<LoadoutItemInfoOverrideController>();

        [Header("Stats Controller")]

        [Tooltip("The loadout item label.")]
        [SerializeField]
        protected UVCText labelText;
        public UVCText LabelText
        {
            get { return labelText; }
            set { labelText = value; }
        }

        [Tooltip("The loadout item description.")]
        [SerializeField]
        protected UVCText descriptionText;
        public UVCText DescriptionText
        {
            get { return descriptionText; }
            set { descriptionText = value; }
        }

        [Tooltip("The icon image for the loadout item.")]
        [SerializeField]
        protected Image iconImage;
        public Image IconImage
        {
            get { return iconImage; }
            set { iconImage = value; }
        }

        [Tooltip("The prefab for displaying a stat about the loadout item.")]
        [SerializeField]
        protected StatsInstance statsInstancePrefab;
        protected List<StatsInstance> statsInstances = new List<StatsInstance>();

        [Tooltip("The parent for stats UI.")]
        [SerializeField]
        protected Transform statsInstanceParent;


        protected virtual void Reset()
        {
            loadoutManager = FindObjectOfType<LoadoutManager>();
        }

        protected virtual void Awake()
        {
            overrideControllers = new List<LoadoutItemInfoOverrideController>(transform.GetComponentsInChildren<LoadoutItemInfoOverrideController>());
            foreach (LoadoutItemInfoOverrideController overrideController in overrideControllers)
            {
                overrideController.ItemInfoController = this;
            }

            loadoutManager.onLoadoutChanged.AddListener(UpdateInfo);
        }


        protected virtual void Show()
        {
            UIHandle.SetActive(true);
        }


        protected virtual void Hide()
        {
            UIHandle.SetActive(false);
        }


        /// <summary>
        /// Update the loadout item info.
        /// </summary>
        public virtual void UpdateInfo()
        {
            ClearStatsInstances();
            Show();
        }


        /// <summary>
        /// Get a new stats instance to display something about the loadout item.
        /// </summary>
        /// <returns></returns>
        public virtual StatsInstance GetStatsInstance()
        {
            foreach(StatsInstance statsInstance in statsInstances)
            {
                if (!statsInstance.gameObject.activeSelf)
                {
                    statsInstance.gameObject.SetActive(true);
                    return statsInstance;
                }
            }

            StatsInstance newStatsInstance = Instantiate(statsInstancePrefab, statsInstanceParent);
            statsInstances.Add(newStatsInstance);

            return newStatsInstance;
        }


        /// <summary>
        /// Clear all the stats items.
        /// </summary>
        public virtual void ClearStatsInstances()
        {
            foreach (StatsInstance statsInstance in statsInstances)
            {
                statsInstance.gameObject.SetActive(false);
            }
        }


        /// <summary>
        /// Set the label for the loadout item.
        /// </summary>
        /// <param name="text">The label content.</param>
        public virtual void SetLabel(string text)
        {
            if (labelText != null) labelText.text = text;
        }


        /// <summary>
        /// Set the description for the loadout item.
        /// </summary>
        /// <param name="text">The description content.</param>
        public virtual void SetDescription(string text)
        {
            if (descriptionText != null) descriptionText.text = text;
        }


        /// <summary>
        /// Set the icon sprite for the loadout item.
        /// </summary>
        /// <param name="icon">The icon sprite.</param>
        public virtual void SetIcon(Sprite icon)
        {
            if (iconImage != null) iconImage.sprite = icon;
        }
    }
}

