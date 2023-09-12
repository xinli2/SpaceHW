using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Stores a reference to a renderer and its color key, and makes it easy to modify its material properties.
    /// </summary>
    [System.Serializable]
    public class AnimatedRenderer
    {
        public Renderer renderer;
        public string colorKey = "_Color";
        public bool preserveAlpha = false;
        protected float baseAlpha = 1;
        public float BaseAlpha { get { return baseAlpha; } }

        public AnimatedRenderer(Renderer renderer, string colorKey)
        {
            this.renderer = renderer;
            this.colorKey = colorKey;
            baseAlpha = renderer.material.GetColor(colorKey).a;
        }

        public void SetAlpha(float alpha)
        {
            Color c = renderer.material.GetColor(colorKey);
            c.a = alpha;
            renderer.material.SetColor(colorKey, c);
        }

        public void SetColor(Color newColor)
        {
            renderer.material.SetColor(colorKey, newColor);
        }
    }
}
