using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Make a UI object follow the fill threshold of a fill image.
    /// </summary>
    public class FillBarFollower : MonoBehaviour
    {

        [SerializeField] protected Image fillBar;
        [SerializeField] protected RectTransform follower;


        protected virtual void Update()
        {
            Image.FillMethod fillMethod = fillBar.fillMethod;

            switch (fillMethod)
            {
                case Image.FillMethod.Horizontal:

                    if (fillBar.fillOrigin == 3)    // Left
                    {
                        follower.anchoredPosition = fillBar.rectTransform.anchoredPosition - Vector2.right * (fillBar.rectTransform.rect.width / 2) + Vector2.right * fillBar.fillAmount * fillBar.rectTransform.rect.width;
                    }
                    else if (fillBar.fillOrigin == 1)   // Right
                    {
                        follower.anchoredPosition = fillBar.rectTransform.anchoredPosition + Vector2.right * (fillBar.rectTransform.rect.width / 2) - Vector2.right * fillBar.fillAmount * fillBar.rectTransform.rect.width;
                    }

                    break;

                case Image.FillMethod.Vertical:

                    if (fillBar.fillOrigin == 0)    // Bottom
                    {
                        follower.anchoredPosition = fillBar.rectTransform.anchoredPosition - Vector2.up * (fillBar.rectTransform.rect.height / 2) + Vector2.up * fillBar.fillAmount * fillBar.rectTransform.rect.height;
                    }
                    else if (fillBar.fillOrigin == 2)   // Top
                    {
                        follower.anchoredPosition = fillBar.rectTransform.anchoredPosition + Vector2.up * (fillBar.rectTransform.rect.height / 2) - Vector2.up * fillBar.fillAmount * fillBar.rectTransform.rect.height;
                    }

                    break;
            }
        }
    }
}
