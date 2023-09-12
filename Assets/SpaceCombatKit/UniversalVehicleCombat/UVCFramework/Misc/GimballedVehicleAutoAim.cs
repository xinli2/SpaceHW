using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat.Mechs
{
    /// <summary>
    /// Auto aim the gimbal of a vehicle when there is a target selected.
    /// </summary>
    public class GimballedVehicleAutoAim : MonoBehaviour
    {

        [Tooltip("A reference to the gimbal controller.")]
        [SerializeField]
        protected GimballedVehicleController gimballedVehicleController;

        [SerializeField]
        protected TargetSelector targetSelector;

        [SerializeField]
        protected bool autoAimEnabled = true;
        public bool AutoAimEnabled { get { return autoAimEnabled; } }
        
        protected Vector3 offset = Vector3.zero;

        protected Vector3 tempOffset = Vector3.zero;


        protected virtual void Reset()
        {
            gimballedVehicleController = GetComponent<GimballedVehicleController>();
            targetSelector = GetComponentInChildren<TargetSelector>();
        }

        protected virtual void Awake()
        {
            // Subscribe to camera view target selection events
            CameraViewTarget[] cameraViewTargets = transform.GetComponentsInChildren<CameraViewTarget>();
            foreach (CameraViewTarget viewTarget in cameraViewTargets)
            {
                viewTarget.onSelected.AddListener(delegate { OnCameraTargetChanged(viewTarget); });
            }

            gimballedVehicleController.onLateUpdateComplete.AddListener(Aim);
        }

        public virtual void EnableAutoAim()
        {
            autoAimEnabled = true;
        }

        public virtual void DisableAutoAim()
        {
            autoAimEnabled = false;
        }

        protected virtual void OnCameraTargetChanged(CameraViewTarget newTarget)
        {
            offset = gimballedVehicleController.GimbalController.VerticalPivot.transform.InverseTransformPoint(newTarget.transform.position);
        }
        
        public void Aim()
        {
            if (autoAimEnabled)
            {
                if (targetSelector.SelectedTarget != null)
                {
                    gimballedVehicleController.SetRotationInputEnabled(false);

                    float angle;

                    Vector3 pos = targetSelector.SelectedTarget.transform.TransformPoint(targetSelector.SelectedTarget.TrackingBounds.center);
                    gimballedVehicleController.GimbalController.TrackPosition(pos + tempOffset, out angle, true);

                    pos -= gimballedVehicleController.GimbalController.VerticalPivot.TransformDirection(offset); // Must be done twice because the TransformDirection must be when the aim controller is aiming at the target.
                    gimballedVehicleController.GimbalController.TrackPosition(pos + tempOffset, out angle, true);
                    gimballedVehicleController.UpdateIndependentGimbalRotation();
                }
                else
                {
                    gimballedVehicleController.SetRotationInputEnabled(true);
                }

                Vector3 origin = gimballedVehicleController.GimbalController.VerticalPivot.position;
                Vector3 fwd = gimballedVehicleController.GimbalController.VerticalPivot.forward;
            }
            else
            {
                gimballedVehicleController.SetRotationInputEnabled(true);
            }
        }
    }
}
