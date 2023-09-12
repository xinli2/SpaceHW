using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Manages the enabling and disabling of shadow caster doubles mounted on a module mount.
    /// </summary>
    public class ModuleMountShadowCasterAnchor : MonoBehaviour
    {
        [Tooltip("The module mount that the shadow caster double module is mounted.")]
        [SerializeField]
        protected ModuleMount moduleMount;

        [Tooltip("The anchor transform for the shadow caster double.")]
        [SerializeField]
        protected Transform shadowCasterDoubleAnchor;


        protected virtual void Awake()
        {
            moduleMount.onModuleMounted.AddListener(OnModuleMounted);
            moduleMount.onModuleUnmounted.AddListener(OnModuleUnmounted);
        }


        // Called when a module is mounted at the module mount
        protected virtual void OnModuleMounted(Module module)
        {
            ShadowCasterDouble shadowCasterDouble = module.GetComponentInChildren<ShadowCasterDouble>();
            if (shadowCasterDouble != null)
            {
                shadowCasterDouble.AnchorDouble(shadowCasterDoubleAnchor);
            }
        }

        // Called when a module is unmounted at the module mount
        protected virtual void OnModuleUnmounted(Module module)
        {
            ShadowCasterDouble shadowCasterDouble = module.GetComponentInChildren<ShadowCasterDouble>();
            if (shadowCasterDouble != null)
            {
                shadowCasterDouble.UnanchorDouble();
            }
        }
    }
}
