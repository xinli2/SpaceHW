using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace VSX.UniversalVehicleCombat
{

    [System.Serializable]
    public class OnButtonSelectedEventHandler : UnityEvent<int> { }

    public class ButtonsListController : MonoBehaviour
    {

        [SerializeField]
        protected ButtonController buttonPrefab;

        [SerializeField]
        protected Transform buttonsParent;

        [SerializeField]
        protected AudioSource buttonPointerEnterAudio;

        [SerializeField]
        protected AudioSource buttonClickAudio;

        protected List<ButtonController> buttonControllers = new List<ButtonController>();
        public List<ButtonController> ButtonControllers { get { return buttonControllers; } }

        public OnButtonSelectedEventHandler onButtonClicked;


        public virtual void SetNumButtons(int numButtons)
        {
            int diff = numButtons - buttonControllers.Count;
 
            if (diff > 0)
            {
                for (int i = 0; i < diff; ++i)
                {
                    ButtonController buttonController = Instantiate(buttonPrefab, buttonsParent) as ButtonController;
                    buttonController.transform.SetParent(buttonsParent);
                    buttonController.transform.localPosition = Vector3.zero;
                    buttonController.transform.localRotation = Quaternion.identity;
                    buttonController.transform.localScale = new Vector3(1f, 1f, 1f);

                    buttonControllers.Add(buttonController);

                    buttonController.ID = buttonControllers.Count - 1;

                    // Add events to the button
                    int index = buttonControllers.Count - 1;
                    buttonController.Button.onClick.AddListener(delegate { OnButtonClicked(index); });

                    if (buttonClickAudio != null)
                    {
                        buttonController.Button.onClick.AddListener(buttonClickAudio.Play);
                    }

                    if (buttonPointerEnterAudio != null)
                    {
                        EventTrigger eventTrigger = buttonController.gameObject.GetComponentInChildren<EventTrigger>();
                        if (eventTrigger == null)
                        {
                            eventTrigger = buttonController.Button.gameObject.AddComponent<EventTrigger>();
                        }
                        
                        EventTrigger.Entry entry = new EventTrigger.Entry();
                        entry.eventID = EventTriggerType.PointerEnter;
                        entry.callback.AddListener((data) => { buttonPointerEnterAudio.Play(); });
                        eventTrigger.triggers.Add(entry);

                    }

                }
            }
            else
            {
                for (int i = 0; i < Mathf.Abs(diff); ++i)
                {
                    int nextIndex = numButtons + i;
                    buttonControllers[nextIndex].gameObject.SetActive(false);
                }
            }

            // Activate the buttons
            for (int i = 0; i < numButtons; ++i)
            {
                buttonControllers[i].gameObject.SetActive(true);
            }
        }


        public void SetVisibleButtons(List<int> visibleIndexes)
        {

            for (int i = 0; i < buttonControllers.Count; ++i)
            {
                buttonControllers[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < visibleIndexes.Count; ++i)
            {
                buttonControllers[visibleIndexes[i]].gameObject.SetActive(true);
            }
        }

        public virtual void SetButtonSelected(int index)
        {
            for (int i = 0; i < buttonControllers.Count; ++i)
            {
                if (i == index)
                {
                    buttonControllers[i].SetSelected(true);
                }
                else
                {
                    buttonControllers[i].SetSelected(false);
                }
            }
            
        }

        protected virtual void OnButtonClicked(int index)
        {
            onButtonClicked.Invoke(index);
        }
    }
}

