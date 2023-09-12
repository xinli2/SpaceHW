using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Controls the rendering of a vehicle with separated (static) interior and (dynamic) exterior such that they appear to be a single object and character controllers can appear
    /// to move around inside the vehicle while it is moving. 
    /// </summary>
    [DefaultExecutionOrder(-30)]
    public class VehicleInteriorRenderer : MonoBehaviour
    {
        [Tooltip("The player in the scene.")]
        [SerializeField]
        protected GameAgent player;
        protected Vector3 playerPosition;
        protected bool updatePlayerPositionOnTeleported = true;

        [Tooltip("The world position where the vehicle interior gameobject should be located when active.")]
        [SerializeField]
        protected Vector3 interiorSpawnPosition = Vector3.zero;
        public Vector3 InteriorSpawnPosition { get { return interiorSpawnPosition; } }

        protected VehicleInterior interior; // The currently rendered interior

        [Tooltip("The main camera of the scene.")]
        [SerializeField]
        protected Camera m_Camera;
        protected LayerMask defaultMask;

        [Tooltip("The camera that renders the exterior (the world outside the vehicle).")]
        [SerializeField]
        protected Camera exteriorCamera;

        [Tooltip("The layer mask that defines objects that are only rendered by the interior camera.")]
        [SerializeField]
        public LayerMask interiorLayerMask;
        protected LayerMask exteriorLayerMask;

        [Tooltip("The transform of the light that represents the main light of the scene.")]      
        [SerializeField]
        protected Transform mainLightTransform;

        [Tooltip("The transform of the light that represents the main light of the scene for the interior of the vehicle.")]
        [SerializeField]
        protected Transform interiorMainLightTransform;

        public UnityEvent onOperationStarted;
        public UnityEvent onOperationStopped;

        protected bool running = false;

        public static VehicleInteriorRenderer Instance;

        protected List<VehicleInteriorVolume> interiorVolumes = new List<VehicleInteriorVolume>();



        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            defaultMask = m_Camera.cullingMask;
            exteriorLayerMask = defaultMask & ~interiorLayerMask;   // Subtract interior from default
            
        }


        protected virtual void Start()
        {
            if (player == null && GameAgentManager.Instance != null)
            {
                player = GameAgentManager.Instance.FocusedGameAgent;
            }

            if (player != null && player.Character != null) player.Character.onCharacterTeleported.AddListener(OnPlayerTeleported);
        }


        /// <summary>
        /// Register an interior volume so that the player position can be checked as to whether they are inside it.
        /// </summary>
        /// <param name="volume">The interior volume.</param>
        public virtual void Register(VehicleInteriorVolume volume)
        {
            if (interiorVolumes.IndexOf(volume) == -1) interiorVolumes.Add(volume);
        }


        public virtual bool IsInterior(Vector3 worldPosition)
        {
            // Check if should be running or not
            foreach (VehicleInteriorVolume interiorVolume in interiorVolumes)
            {
                if (interiorVolume.IsInsideVolume(worldPosition))
                {
                    return true;
                }
            }

            return false;
        }



        // Check if the player is inside a vehicle
        protected virtual bool IsPlayerInsideVehicle (out VehicleInteriorVolume volume)
        {
            volume = null;

            if (player.Vehicle != player.Character) return false;

            // Check if should be running or not
            foreach (VehicleInteriorVolume interiorVolume in interiorVolumes)
            {
                if (interiorVolume.IsInsideVolume(playerPosition - (player.Character.Height / 2) * Vector3.up))
                {
                    volume = interiorVolume;
                    return true;
                }
            }

            return false;
        }


        protected virtual void LateUpdate()
        {
            UpdatePlayerPosition();
            UpdateState();
        }


        // Called when the player character is moved from one position to another.
        protected virtual void OnPlayerTeleported()
        {
            if (updatePlayerPositionOnTeleported) SetPlayerPosition(player.Character.Center);
        }


        protected virtual void UpdatePlayerPosition()
        {
            // Get player position
            if (running)
            {
                playerPosition = interior.Vehicle.transform.TransformPoint(interior.transform.InverseTransformPoint(player.Character.transform.position));
            }
            else
            {
                playerPosition = player.Character.Center;
            }
        }


        /// <summary>
        /// Set the player position in the scene.
        /// </summary>
        /// <param name="position">The world position of the player.</param>
        public void SetPlayerPosition(Vector3 position)
        {
            playerPosition = position;

            UpdateState();
        }


        /// <summary>
        /// Update the interior rendering.
        /// </summary>
        public virtual void UpdateState()
        {
            
            VehicleInteriorVolume currentVolume;
            if (IsPlayerInsideVehicle(out currentVolume))
            {

                if (!running)
                {
                    StartOperation(currentVolume);
                }
            }
            else
            {
                if (running)
                {
                    StopOperation();
                }

            }

            if (running)
            {
                Quaternion localRot = Quaternion.Inverse(interior.transform.rotation) * m_Camera.transform.rotation;
                exteriorCamera.transform.rotation = interior.Vehicle.transform.rotation * localRot;

                exteriorCamera.transform.position = interior.Vehicle.transform.TransformPoint(interior.transform.InverseTransformPoint(m_Camera.transform.position));

                if (mainLightTransform != null && interiorMainLightTransform != null)
                {
                    localRot = Quaternion.Inverse(interior.Vehicle.transform.rotation) * mainLightTransform.rotation;
                    interiorMainLightTransform.rotation = interior.transform.rotation * localRot;
                }
            }
            else
            {
                exteriorCamera.transform.position = m_Camera.transform.position;
                exteriorCamera.transform.rotation = m_Camera.transform.rotation;
            }
        }


        // Start rendering interior
        protected virtual void StartOperation(VehicleInteriorVolume volume)
        {
            if (running)
            {
                StopOperation();
            }

            interior = volume.VehicleInterior;

            running = true;

            interior.OnPlayerEntered();

            exteriorCamera.enabled = true;
            m_Camera.clearFlags = CameraClearFlags.Depth;
            m_Camera.cullingMask = interiorLayerMask;

            Vector3 pos = interior.transform.TransformPoint(interior.Vehicle.transform.InverseTransformPoint(player.Character.transform.position));
            Quaternion rot = (Quaternion.Inverse(interior.Vehicle.transform.rotation) * player.Character.transform.rotation) * interior.transform.rotation;
            TeleportCharacterWithoutUpdatePosition(pos, rot);

            CinemachineVirtualCamera virtualCam = m_Camera.GetComponent<CinemachineBrain>().ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
            virtualCam.ForceCameraPosition(virtualCam.transform.position, virtualCam.transform.rotation);
            m_Camera.transform.position = virtualCam.transform.position;
            m_Camera.transform.rotation = virtualCam.transform.rotation;

            UpdatePlayerPosition();

            onOperationStarted.Invoke();

        }

        
        // Stop rendering interior
        protected virtual void StopOperation()
        {

            if (!running) return;

            running = false;

            Quaternion rot = (Quaternion.Inverse(interior.transform.rotation) * player.Character.transform.rotation) * interior.Vehicle.transform.rotation;
            TeleportCharacterWithoutUpdatePosition(playerPosition, rot);

            if (interior != null)
            {
                interior.OnPlayerExited();
            }
            interior = null;

            exteriorCamera.enabled = false;
            m_Camera.clearFlags = CameraClearFlags.Skybox;
            m_Camera.cullingMask = defaultMask;
            m_Camera.transform.position = exteriorCamera.transform.position;

            CinemachineVirtualCamera virtualCam = m_Camera.GetComponent<CinemachineBrain>().ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
            virtualCam.ForceCameraPosition(virtualCam.transform.position, virtualCam.transform.rotation);

            onOperationStopped.Invoke();

        }


        // Teleport the character without updating the tracked player position.
        protected virtual void TeleportCharacterWithoutUpdatePosition(Vector3 position, Quaternion rotation)
        {
            updatePlayerPositionOnTeleported = false;
            player.Character.SetPositionAndRotation(position, rotation);
            updatePlayerPositionOnTeleported = true;
        }
    }
}

