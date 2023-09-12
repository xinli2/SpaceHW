using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// A turret is an gimballed weapon that can be operated manually by the player or completely independently.
    /// </summary>
    public class Turret : MonoBehaviour
    {
        [Tooltip("The turret control mode.")]
        [SerializeField]
        protected TurretMode turretMode;
        public TurretMode TurretMode
        {
            get { return turretMode; }
            set 
            { 
                turretMode = value;
                switch (turretMode)
                {
                    case TurretMode.Manual:

                        weapon.SetAimingEnabled(true);
                        break;

                    default:

                        weapon.SetAimingEnabled(false);
                        break;
                }
            }
        }

        [Tooltip("Whether the turret is locked to the target or smoothly moves toward it.")]
        [SerializeField]
        protected bool lockToTarget = false;


        [Header("Aim Assist")]

        [Tooltip("Whether the turret snaps toward the target when within a certain angle.")]
        [SerializeField]
        protected bool aimAssist = true;

        [Tooltip("The angle within which the turret snaps toward the target.")]
        [SerializeField]
        protected float aimAssistAngleThreshold = 3;


        [Header("Components")]

        [Tooltip("The turret gimbal controller.")]
        [SerializeField]
        protected GimbalController gimbalController;
        public GimbalController GimbalController { get { return gimbalController; } }

        [Tooltip("The weapon component for this turret.")]
        [SerializeField]
        protected Weapon weapon;
        public Weapon Weapon
        {
            get { return weapon; }
            set { weapon = value; }
        }

        [Tooltip("The turret's current target.")]
        [SerializeField]
        protected Trackable target;

        [Tooltip("The target selector for the turret's own target acquisition.")]
        [SerializeField]
        protected TargetSelector targetSelector;
        public TargetSelector TargetSelector
        {
            get { return targetSelector; }
            set { targetSelector = value; }
        }

        [Tooltip("Whether to lead the target when aiming or not.")]
        [SerializeField]
        protected bool leadTarget = true;

        [Tooltip("Whether the turret rotates back to the center when no target is present.")]
        [SerializeField]
        protected bool noTargetReturnToCenter = true;

        protected float firingAngle;
       


        // Called when this component is first added to a gameobject, or when it is reset in the inspector
        protected virtual void Reset()
        {

            // Get gimbal controller
            gimbalController = GetComponentInChildren<GimbalController>();
            if (gimbalController == null)
            {
                gimbalController = gameObject.AddComponent<GimbalController>();
            }

            // Get weapon
            weapon = GetComponentInChildren<Weapon>();
          
            // Get/add a target selector
            targetSelector = GetComponentInChildren<TargetSelector>();

        }


        protected virtual void Awake()
        {
            TurretMode = turretMode;
        }


        public virtual void SetTarget(Trackable target)
        {
            this.target = target;
        }


        protected virtual void UpdateTarget()
        {
            if (targetSelector != null && targetSelector.SelectedTarget != target) 
                SetTarget(targetSelector.SelectedTarget);
        }


        public virtual float AngleToTarget()
        {
            Vector3 targetPosition = target.transform.position;
            if (leadTarget && target.Rigidbody != null)
            {
                targetPosition = TargetLeader.GetLeadPosition(gimbalController.VerticalPivot.position, weapon.Speed, targetPosition, target.Rigidbody.velocity);
            }

            return gimbalController.AngleToTarget(targetPosition);
        }


        protected virtual void TrackTarget()
        {
            if (target == null) return;
            
            Vector3 targetPosition = target.transform.position;
            if (leadTarget && target.Rigidbody != null)
            {
                targetPosition = TargetLeader.GetLeadPosition(gimbalController.VerticalPivot.position, weapon.Speed, targetPosition, target.Rigidbody.velocity);
            }
            
            gimbalController.TrackPosition(targetPosition, out firingAngle, lockToTarget);

            if (aimAssist)
            {
                if (Vector3.Angle(gimbalController.VerticalPivot.forward, targetPosition - gimbalController.VerticalPivot.position) < aimAssistAngleThreshold)
                {
                    weapon.Aim(targetPosition);
                }
                else
                {
                    weapon.ClearAim();
                }
            }
        }


        protected virtual void TurretControlUpdate() { }


        // Called every frame
        protected virtual void Update()
        {
            switch (turretMode)
            {
                case TurretMode.Auto:

                    TurretControlUpdate();
                    break;

                case TurretMode.Independent:

                    UpdateTarget();
                    TurretControlUpdate();

                    break;
            }
        }
    }
}
