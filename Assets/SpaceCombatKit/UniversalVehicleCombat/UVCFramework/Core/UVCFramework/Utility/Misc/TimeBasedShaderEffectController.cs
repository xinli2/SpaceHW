using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Apply a time based value to a shader effect.
    /// </summary>
    public class TimeBasedShaderEffectController : MonoBehaviour
    {
        [Tooltip("The shader variable to control.")]
        [SerializeField]
        protected string shaderKey = "_UVOffsetY";

        [Tooltip("The renderer with the effect.")]
        [SerializeField]
        protected Renderer m_Renderer;

        [Tooltip("The time based effect multiplier.")]
        [SerializeField]
        protected float effectMultiplier = 1;


        // Called when the script is first added to a gameobject or reset in the inspector
        protected virtual void Reset()
        {
            m_Renderer = GetComponent<Renderer>();
        }

        // Called every frame
        protected virtual void Update()
        {
            if (m_Renderer != null)
            {
                m_Renderer.material.SetFloat(shaderKey, Time.realtimeSinceStartup * effectMultiplier);
            }
        }
    }
}