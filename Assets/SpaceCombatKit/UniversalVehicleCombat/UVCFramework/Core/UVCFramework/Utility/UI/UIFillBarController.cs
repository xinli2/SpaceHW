using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.Utilities.UI
{
    public class UIFillBarController : MonoBehaviour
    {

        [SerializeField]
        protected Image barFill;


        protected virtual void Reset()
        {
            barFill = GetComponentInChildren<Image>();
        }

        public virtual void SetFillAmount(float fillAmount)
        {
            barFill.fillAmount = fillAmount;
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}