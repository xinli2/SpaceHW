using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using VSX.Utilities.UI;

namespace VSX.UniversalVehicleCombat
{
    [System.Serializable]
    public class ButtonControllerText
    {
        public UVCText text;

        public Color unselectedButtonTextColor = Color.gray;

        public Color selectedButtonTextColor = Color.white;

        public bool toUpper = false;
    }

    [System.Serializable]
    public class ButtonControllerIcon
    {
        public Image iconImage;

        public Color unselectedIconColor = Color.gray;

        public Color selectedIconColor = Color.white;
    }

    [System.Serializable]
    public class ButtonControllerButtonImage
    {
        public Image buttonImage;

        public Color unselectedButtonColor = Color.white;

        public Color selectedButtonColor = Color.white;

        public Sprite selectedButtonSprite;

        public Sprite unselectedButtonSprite;
    }

    public class ButtonController : MonoBehaviour
    {

        [SerializeField]
        protected Button button;
        public Button Button { get { return button; } }

        public List<GameObject> selectedActivatedObjects = new List<GameObject>();
        public List<GameObject> unselectedActivatedObjects = new List<GameObject>();

        public List<GameObject> deepSelectedActivatedObjects = new List<GameObject>();
        public List<GameObject> deepUnselectedItemActivatedObjects = new List<GameObject>();

        [Header("Button Text")]

        [SerializeField]
        protected List<ButtonControllerText> buttonTexts = new List<ButtonControllerText>();

        [Header ("Button Icon")]

        [SerializeField]
        protected List<ButtonControllerIcon> buttonIcons = new List<ButtonControllerIcon>();

        [Header("Button")]

        [SerializeField]
        protected List<ButtonControllerButtonImage> buttonImages = new List<ButtonControllerButtonImage>();

        [SerializeField]
        protected int m_ID;
        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        public UnityEvent onClicked;


        public void SetText(int index, string text)
        {
            if (buttonTexts.Count > index)
            {
                buttonTexts[index].text.text = buttonTexts[index].toUpper ? text.ToUpper() : text;
            }
        }

        public void SetIcon(int index, Sprite sprite)
        {
            if (buttonIcons.Count > index)
            {
                buttonIcons[index].iconImage.sprite = sprite;
            }
        }

        public void SetSelected(bool selected)
        {

            foreach(ButtonControllerButtonImage buttonImage in buttonImages)
            {
                buttonImage.buttonImage.color = selected ? buttonImage.selectedButtonColor : buttonImage.unselectedButtonColor;
                buttonImage.buttonImage.sprite = selected ? buttonImage.selectedButtonSprite : buttonImage.unselectedButtonSprite;
            }
            
            foreach(ButtonControllerText buttonText in buttonTexts)
            {
                buttonText.text.color = selected ? buttonText.selectedButtonTextColor : buttonText.unselectedButtonTextColor;
            }

            foreach (ButtonControllerIcon buttonIcon in buttonIcons)
            {
                buttonIcon.iconImage.color = selected ? buttonIcon.selectedIconColor : buttonIcon.unselectedIconColor;
            }

            foreach(GameObject g in selectedActivatedObjects)
            {
                g.SetActive(selected);
            }

            foreach (GameObject g in unselectedActivatedObjects)
            {
                g.SetActive(!selected);
            }
        }

        public void SetDeepSelected(bool deepSelected)
        {

            foreach (GameObject g in deepSelectedActivatedObjects)
            {
                g.SetActive(deepSelected);
            }

            foreach (GameObject g in deepUnselectedItemActivatedObjects)
            {
                g.SetActive(!deepSelected);
            }
        }

        public virtual void OnClicked()
        {
            onClicked.Invoke();
        }
    }
}