using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;
using VSX.UniversalVehicleCombat.Radar;
using VSX.Utilities.UI;

namespace VSX.UniversalVehicleCombat
{
    [DefaultExecutionOrder(50)]
    public class AimController : ModuleManager, ICamerasUser
    {

        [SerializeField]
        protected Transform aimOrigin;

        protected Vector3 aimOriginPosition;
        protected Vector3 aimDirection;
        protected Vector3 aimTarget;

        [SerializeField]
        protected bool useCameraAsRaycastOrigin = true;

        [SerializeField]
        protected CameraTarget cameraTarget;

        [SerializeField]
        protected int aimCameraIndex = 0;
        protected Camera aimCamera;

        [Header("Aim Assist")]

        [SerializeField]
        protected bool aimAssist = true;
        public bool AimAssist
        {
            get { return aimAssist; }
            set 
            { 
                aimAssist = value;
                foreach (IAimer aimer in aimers)
                {
                    aimer.ClearAim();
                }
            }
        }

        protected List<IAimer> aimers = new List<IAimer>();

        [SerializeField]
        protected float aimAssistAngle = 3.3f;

        [SerializeField]
        protected float defaultAimAssistRange = 1000;
        protected float aimAssistRange;

        [SerializeField]
        protected Color noAimAssistColor = Color.white;

        [SerializeField]
        protected Color aimAssistColor = Color.red;

        [SerializeField]
        protected UIColorManager hudCursorColorManager;

        [Header("Raycast Aiming")]

        [SerializeField]
        protected bool raycastAim = true;
        public bool RaycastAim
        {
            get { return raycastAim; }
            set
            {
                raycastAim = value;

                if (!raycastAim)
                {
                    foreach (IAimer aimer in aimers)
                    {
                        aimer.ClearAim();
                    }

                    if (aimPositionMarker != null) aimPositionMarker.position = aimOriginPosition + aimDirection * aimAssistRange;
                }
            }
        }

        [SerializeField]
        protected LayerMask raycastAimMask;

        [SerializeField]
        protected bool ignoreTriggerColliders = true;

        [SerializeField]
        protected Transform aimPositionMarker;

        [Header("Gimbal Aiming")]

        [Tooltip("Whether the gimbal will aim at the selected target.")]
        [SerializeField]
        protected bool gimbalAim = true;

        [SerializeField]
        protected GimbalController aimingGimbal;

        [SerializeField]
        protected bool lockGimbalToTarget;

        [Header("Cursor Aiming")]

        [SerializeField]
        protected bool cursorAimingEnabled = true;

        [SerializeField]
        protected HUDCursor cursor;

        [Header("Aim-Based Target Selection")]

        [SerializeField]
        protected bool aimTargetSelectionEnabled = true;

        [SerializeField]
        protected float targetSelectionAngle = 3;

        [SerializeField]
        protected Tracker tracker;

        [SerializeField]
        protected TargetSelector targetSelector;

        [SerializeField]
        protected Weapons weapons;

        [Header("Components")]

        [SerializeField]
        protected Rigidbody m_Rigidbody;


        protected virtual void Reset()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            tracker = GetComponent<Tracker>();
            cameraTarget = GetComponent<CameraTarget>();

            targetSelector = transform.GetComponentInChildren<TargetSelector>();

            cursor = transform.GetComponentInChildren<HUDCursor>();

            if (cursor != null)
            {
                hudCursorColorManager = cursor.GetComponentInChildren<UIColorManager>();
            }

            weapons = GetComponent<Weapons>();

            aimOrigin = transform;

            raycastAimMask = ~0;

            aimingGimbal = GetComponentInChildren<GimbalController>();
        }

        public void SetCameras(List<Camera> cameras)
        {
            if (cameras.Count > aimCameraIndex)
            {
                aimCamera = cameras[aimCameraIndex];
                if (useCameraAsRaycastOrigin) aimOrigin = aimCamera.transform;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (aimOrigin == null) aimOrigin = transform;
        }


        // Called when a module is mounted on one of the vehicle's module mounts.
        protected override void OnModuleMounted(Module module)
        {
            // Store aim assist reference
            IAimer aimer = module.GetComponentInChildren<IAimer>();
            if (aimer != null)
            {
                aimers.Add(aimer);
            }
        }


        // Called when a module is unmounted from one of the vehicle's module mounts.
        protected override void OnModuleUnmounted(Module module)
        {
            // Remove aim assist reference
            IAimer aimer = module.GetComponentInChildren<IAimer>();
            if (aimer != null)
            {
                if (aimers.Contains(aimer))
                {
                    aimers.Remove(aimer);
                }
            }
        }


        protected virtual bool AimAssistUpdate()
        {
            if (hudCursorColorManager != null) hudCursorColorManager.SetColor(noAimAssistColor);

            if (targetSelector == null || targetSelector.SelectedTarget == null) return false;

            aimTarget = targetSelector.SelectedTarget.transform.position;

            if (weapons != null)
            {
                aimTarget = weapons.GetAverageLeadTargetPosition(targetSelector.SelectedTarget.transform.TransformPoint(targetSelector.SelectedTarget.TrackingBounds.center), 
                                                                    targetSelector.SelectedTarget.Rigidbody != null ? targetSelector.SelectedTarget.Rigidbody.velocity : Vector3.zero);
            }

            if (Vector3.Distance(aimOrigin.position, aimTarget) > aimAssistRange)
            {
                return false;
            }
            
            if (Vector3.Angle(aimDirection, (aimTarget - aimOrigin.position)) > aimAssistAngle)
            {
                return false;
            }

            /*
            if (!HasLineOfSight(aimTarget, targetSelector.SelectedTarget))
            {
                return false;
            }
            */

            if (hudCursorColorManager != null)
            {
                hudCursorColorManager.SetColor(aimAssistColor);
            }

            // Aim assist found
            if (aimAssist)
            {
                foreach (IAimer aimer in aimers)
                {
                    aimer.Aim(aimTarget);
                }

                return true;
            }
            else
            {
                return false;
            }
        }


        public virtual void RaycastAimUpdate()
        {

            aimTarget = aimOrigin.position + aimDirection.normalized * aimAssistRange;

            // Get all raycast hits

            RaycastHit[] hits = Physics.RaycastAll(aimOrigin.position, aimDirection, aimAssistRange, raycastAimMask,
                                                        ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
                
            
            // Sort hits by distance

            List<RaycastHit> sortedHits = SortRaycastHitsByDistance(hits);

            
            // Discard hits on self and find one that is valid

            for (int i = 0; i < sortedHits.Count; ++i)
            {
                if (sortedHits[i].collider.attachedRigidbody == m_Rigidbody)
                {
                    continue;
                }

                aimTarget = sortedHits[i].point;

                break;
            }
            
            
            foreach (IAimer aimer in aimers)
            {
                aimer.Aim(aimTarget);
            }

            if (aimPositionMarker != null) aimPositionMarker.position = aimTarget;

            // Update target selection

            if (aimTargetSelectionEnabled && targetSelector != null)
            {
                for (int i = 0; i < tracker.Targets.Count; ++i)
                {
                    Vector3 toTarget = tracker.Targets[i].transform.position - aimOriginPosition;
                    if (Vector3.Angle(toTarget, aimDirection) < targetSelectionAngle)
                    {
                        targetSelector.Select(tracker.Targets[i]);
                        break;
                    }
                }
            }
        }

        /*
        bool HasLineOfSight(Vector3 aimTarget, Trackable target)
        {

            Vector3 raycastDirection = (aimTarget - aimOrigin.position).normalized;

            float raycastLength = (aimTarget - aimOrigin.position).magnitude;

            // Get all raycast hits
            RaycastHit[] hits = Physics.RaycastAll(aimOrigin.position, raycastDirection, raycastLength, raycastAimMask,
                                                        ignoreTriggerColliders ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);

            // Sort hits by distance
            List<RaycastHit> sortedHits = SortRaycastHitsByDistance(hits);

            // Discard hits on self and find one that is valid
            for (int i = 0; i < sortedHits.Count; ++i)
            {
                if (sortedHits[i].collider.attachedRigidbody != null)
                {
                    // Ignore hits on self
                    if (sortedHits[i].collider.attachedRigidbody == m_Rigidbody)
                    {
                        continue;
                    }

                    // Ignore hits on target
                    if (sortedHits[i].collider.attachedRigidbody == target.Rigidbody)
                    {
                        continue;
                    }
                }


                // If first valid hit is less than distance to aim target, it's blocked
                if (sortedHits[i].distance < Vector3.Distance(aimTarget, aimOrigin.position) - 0.0001f)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }
        */

        protected virtual void GimbalAimUpdate()
        {
            if (aimingGimbal == null) return;

            if (targetSelector != null && targetSelector.SelectedTarget != null)
            {
                float angle;
                aimingGimbal.TrackPosition(targetSelector.SelectedTarget.transform.position, out angle, lockGimbalToTarget);
            }
        }


        protected virtual void LateUpdate()
        {
            aimAssistRange = weapons != null ? weapons.GunWeaponRange : defaultAimAssistRange;

            // Update aim position
            aimOriginPosition = aimOrigin != null ? aimOrigin.position : transform.position;

            // Update aim direction
            aimDirection = aimOrigin != null ? aimOrigin.forward : transform.forward;
            if (cursorAimingEnabled && cursor != null)
            {
                aimDirection = cursor.AimDirection;
            }

            // Aim assist
            bool aimAssistFound = AimAssistUpdate();

            // Raycast aim
            if (!aimAssistFound && raycastAim) RaycastAimUpdate();

            // Gimbal aim
            if (gimbalAim) GimbalAimUpdate();
        }


        public List<RaycastHit> SortRaycastHitsByDistance(RaycastHit[] hits)
        {
            List<RaycastHit> sortedHits = new List<RaycastHit>();

            for (int i = 0; i < hits.Length; ++i)
            {
                if (sortedHits.Count == 0)
                {
                    sortedHits.Add(hits[i]);
                }
                else
                {
                    for (int j = 0; j < sortedHits.Count; ++j)
                    {

                        if (sortedHits[j].distance > hits[i].distance)
                        {
                            sortedHits.Insert(j, hits[i]);
                            break;
                        }

                        if (j == sortedHits.Count - 1)
                        {
                            sortedHits.Add(hits[i]);
                            break;
                        }
                    }
                }
            }

            return sortedHits;
        }
    }
}
