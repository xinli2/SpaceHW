using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VSX.Utilities.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Controller for displaying info for a single module on the HUD.
    /// </summary>
    public class HUDModuleDisplayItem : MonoBehaviour
    {

        [Tooltip("The gameobject to toggle to show/hide the UI.")]
        [SerializeField]
        protected Transform UIHandle;

        [Tooltip("The text controller for displaying the module label.")]
        [SerializeField]
        protected UVCText labelText;

        [Tooltip("The index of the label you want to display in the Module's Labels list.")]
        [SerializeField]
        protected int moduleLabelIndex = 0;

        [Tooltip("The Image that will display the module sprite.")]
        [SerializeField]
        protected Image m_Image;

        [Tooltip("The index of the sprite (in the Module Sprites list) to display for this module.")]
        [SerializeField]
        protected int imageSpriteIndex = 1;

        [Tooltip("Whether to disable the image if no sprite is found at the specified index.")]
        [SerializeField]
        protected bool disableImageIfSpriteNotFound = true;

        [Tooltip("The module to display at the start.")]
        [SerializeField]
        protected Module startingModuleToDisplay = null;

        protected Module displayedModule;


        protected virtual void Reset()
        {
            UIHandle = transform;
        }


        protected virtual void Awake()
        {
            Hide();
        }


        // Called when the scene starts
        protected virtual void Start()
        {
            // Display the starting module if it can be displayed
            if (CanDisplayModule(startingModuleToDisplay))
            {
                DisplayModule(startingModuleToDisplay);
            }
        }


        /// <summary>
        /// Show the UI.
        /// </summary>
        public virtual void Show()
        {
            UIHandle.gameObject.SetActive(true);
        }


        /// <summary>
        /// Hide the UI.
        /// </summary>
        public virtual void Hide()
        {
            UIHandle.gameObject.SetActive(false);
        }


        /// <summary>
        /// Check if a specific module can be displayed.
        /// </summary>
        /// <param name="module">The module to check if can be displayed.</param>
        /// <returns>Whether the module can be displayed.</returns>
        public virtual bool CanDisplayModule(Module module)
        {
            // Check whether the module can be displayed
            return (module != null);
        }

        /// <summary>
        /// Display a module.
        /// </summary>
        /// <param name="module">The module to be displayed.</param>
        public virtual void DisplayModule(Module module)
        {
            if (module != null)
            {
                Show();
                if (labelText != null)
                {
                    labelText.text = module.Labels.Count > moduleLabelIndex ? module.Labels[moduleLabelIndex] : "";
                }

                if (m_Image != null)
                {
                    Sprite sprite = module.Sprites.Count > imageSpriteIndex ? module.Sprites[imageSpriteIndex] : null;
                    if (sprite != null)
                    {
                        m_Image.enabled = true;
                        m_Image.sprite = sprite;
                    }
                    else
                    {
                        if (disableImageIfSpriteNotFound) m_Image.enabled = false;
                    }
                }
                
                displayedModule = module;
            }
            else
            {
                Hide();
                displayedModule = null;
            }
        }

        /// <summary>
        /// Check if this controller is displaying a specific module.
        /// </summary>
        /// <param name="module">The module to check if is displaying.</param>
        /// <returns>Whether the module is being displayed.</returns>
        public virtual bool IsDisplaying(Module module)
        {
            return (displayedModule != null && displayedModule == module);
        }

        /// <summary>
        /// Update the module display.
        /// </summary>
        public virtual void UpdateDisplay() { }
    }
}

