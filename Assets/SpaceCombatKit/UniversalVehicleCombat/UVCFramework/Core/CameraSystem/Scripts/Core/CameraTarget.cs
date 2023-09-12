using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VSX.UniversalVehicleCombat;

namespace VSX.CameraSystem
{
    /// <summary>
    /// Unity Event called when the camera targeting a camera target changes.
    /// </summary>
    [System.Serializable]
    public class OnCameraEntityTargetingEventHandler : UnityEvent<CameraEntity> { }

    /// <summary>
    /// Unity Event called when the camera targeting a camera target changes.
    /// </summary>
    [System.Serializable]
    public class OnCameraTargetingEventHandler : UnityEvent<Camera> { }

    [System.Serializable]
    public class CameraViewTargetEventHandler : UnityEvent<CameraViewTarget> { }

    /// <summary>
    /// A camera target represents an object (such as a character or a vehicle) that a camera entity can follow.
    /// </summary>
    public class CameraTarget : ModuleManager
    {

        protected CameraEntity cameraEntity;
        public CameraEntity CameraEntity { get { return cameraEntity; } }

        protected List<CameraViewTarget> cameraViewTargets = new List<CameraViewTarget>();
        public List <CameraViewTarget> CameraViewTargets
        {
            get { return cameraViewTargets; }
        }

        protected List<ICameraEntityUser> cameraEntityUsers = new List<ICameraEntityUser>();
        protected List<ICamerasUser> camerasUsers = new List<ICamerasUser>();

        [Tooltip("The transform that the camera must always keep in view (for camera collision avoidance/clipping into geometry).")]
        [SerializeField]
        protected Transform lookTarget;
        public Transform LookTarget { get { return lookTarget; } }

        // Called when a camera starts targeting this camera target
        [HideInInspector]
        public OnCameraEntityTargetingEventHandler onCameraEntityTargeting;

        public UnityEvent onCameraFollowing;

        public UnityEvent onCameraStoppedFollowing;

        public CameraViewTargetEventHandler onCameraViewTargetChanged;

        protected Rigidbody m_Rigidbody;
        public Rigidbody Rigidbody { get { return m_Rigidbody; } }



        protected override void Awake()
        {
            base.Awake();

            m_Rigidbody = GetComponent<Rigidbody>();

            // Store the camera views in the right order
            CameraViewTarget[] cameraViewTargetsArray = transform.GetComponentsInChildren<CameraViewTarget>();
            foreach (CameraViewTarget cameraViewTarget in cameraViewTargetsArray)
            {
                // Find the right index to insert the module mount according to its sorting index
                int insertIndex = 0;
                for (int i = 0; i < cameraViewTargets.Count; ++i)
                {
                    if (cameraViewTargets[i].SortingIndex < cameraViewTarget.SortingIndex)
                    {
                        insertIndex = i + 1;
                    }
                }

                // Insert the camera view target into the list
                cameraViewTargets.Insert(insertIndex, cameraViewTarget);
            }

            cameraEntityUsers = new List<ICameraEntityUser>(transform.GetComponentsInChildren<ICameraEntityUser>(true));
            camerasUsers = new List<ICamerasUser>(transform.GetComponentsInChildren<ICamerasUser>(true));

        }


        protected virtual void Reset()
        {
            lookTarget = transform;
        }


        /// <summary>
        /// Set the camera that is currently following this camera target.
        /// </summary>
        /// <param name="cameraEntity">The camera entity that is currently following this camera target.</param>
        public void SetCamera(CameraEntity cameraEntity)
        {
         
            this.cameraEntity = cameraEntity;

            for (int i = 0; i < cameraEntityUsers.Count; ++i)
            {
                cameraEntityUsers[i].SetCameraEntity(cameraEntity);
            }

            for (int i = 0; i < camerasUsers.Count; ++i)
            {
                camerasUsers[i].SetCameras(cameraEntity == null ? new List<Camera>() : cameraEntity.Cameras);
            }

            // Call the events
            onCameraEntityTargeting.Invoke(cameraEntity);

            if (cameraEntity != null)
            {
                onCameraFollowing.Invoke();
            }
            else
            {
                onCameraStoppedFollowing.Invoke();
            }
        }

        protected override void OnModuleMounted(Module module)
        {
            base.OnModuleMounted(module);

            ICameraEntityUser[] moduleCameraEntityUsers = module.GetComponentsInChildren<ICameraEntityUser>();

            foreach(ICameraEntityUser moduleCameraEntityUser in moduleCameraEntityUsers)
            {
                // Check if it's already stored
                bool found = false;
                foreach(ICameraEntityUser storedCameraEntityUser in cameraEntityUsers)
                {
                    if (storedCameraEntityUser == moduleCameraEntityUser)
                    {
                        found = true;
                        break;
                    }
                }

                if (found) continue;

                cameraEntityUsers.Add(moduleCameraEntityUser);
            }
        }

        protected override void OnModuleUnmounted(Module module)
        {
            base.OnModuleUnmounted(module);

            ICameraEntityUser[] moduleCameraEntityUsers = module.GetComponentsInChildren<ICameraEntityUser>();

            foreach (ICameraEntityUser moduleCameraEntityUser in moduleCameraEntityUsers)
            {
                foreach (ICameraEntityUser storedCameraEntityUser in cameraEntityUsers)
                {
                    if (storedCameraEntityUser.Equals(moduleCameraEntityUser))
                    {
                        cameraEntityUsers.Remove(storedCameraEntityUser);
                        break;
                    }
                }
            }
        }
    }
}

