using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.Utilities
{
    /// <summary>
    /// Makes a camera render into a render texture and display it to the screen.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(100)]
    [ExecuteInEditMode]
    public class ShowRenderTexture : MonoBehaviour
    {

        [Tooltip("The render texture to render into and display")]
        [SerializeField]
        protected RenderTexture m_RenderTexture;


        protected virtual void OnPreRender()
        {
            if (m_RenderTexture != null) GetComponent<Camera>().targetTexture = m_RenderTexture;
        }


        protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            GetComponent<Camera>().targetTexture = null;
            if (m_RenderTexture != null) Graphics.Blit(source, (RenderTexture)null);
            RenderTexture.active = destination;
        }
    }
}
