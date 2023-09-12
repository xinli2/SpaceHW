using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace VSX.UniversalVehicleCombat.Loadout
{
    /// <summary>
    /// Manages the behaviour of the camera during loadout.
    /// </summary>
	public class LoadoutCameraController : MonoBehaviour 
	{

		[Tooltip("The loadout UI controller.")]
		[SerializeField]
		protected LoadoutUIController loadoutUIController;

		[Tooltip("The loadout manager.")]
		[SerializeField]
		protected LoadoutManager loadoutManager;

		[Tooltip("The loadout display manager.")]
		[SerializeField]
		protected LoadoutDisplayManager displayManager;

		[Tooltip("The camera to control.")]
		[SerializeField]
        protected Camera m_Camera;

		[Tooltip("Whether to snap the position and rotation of the camera (will smoothly transition if unchecked).")]
		[SerializeField]
		protected bool snapPositionAndRotation = true;

		[Tooltip("The camera move speed.")]
		[SerializeField]
		protected float moveSpeed = 4;

		[Tooltip("The camera rotation speed.")]
		[SerializeField]
		protected float rotationSpeed = 8;



		[Header("View Rotation")]


		[Tooltip("The gimbal that allows the player to rotate the view to inspect a vehicle or module in the loadout.")]
		[SerializeField]
		protected GimbalController viewRotationGimbal;

		[Tooltip("How fast the view rotates.")]
		[SerializeField]
		protected float viewRotationSpeed = 10;

		[Tooltip("How quickly the view rotation responds to input changes. Reduce the value for a smoother rotation.")]
		[SerializeField]
		protected float viewRotationLerpSpeed = 10;

		[Tooltip("The object layers that will block view rotation - usually will be the UI layer.")]
		[SerializeField]
		protected LayerMask viewRotationBlockingLayers;

		protected Vector2 currentGimbalRotationInput;
		protected Vector2 targetGimbalRotationInput;
		protected bool viewRotating = false;


		[Header("Vehicle Focus")]

		[Tooltip("The default angles for the camera view gimbal when entering vehicle selection mode (showing a vehicle in the loadout).")]
		[SerializeField]
		protected Vector3 defaultVehicleFocusAngles;

		[Tooltip("The parent for the camera during vehicle selection.")]
		[SerializeField]
		protected Transform vehicleFocusCameraHolder;

		[Tooltip("The viewport position to place the vehicle in during vehicle selection.")]
		[SerializeField]
		protected Vector2 vehicleViewportPosition = new Vector2(0.5f, 0.5f);

		[Tooltip("The maximum diameter of the vehicle in the viewport during vehicle selection.")]
		[SerializeField]
		protected float maxViewportVehicleDiameter = 0.5f;

		[Tooltip("Whether to adjust the camera distance based on the vehicle size. Will keep the vehicle viewport size approximately within the 'Max Viewport Vehicle Diameter'.")]
		[SerializeField]
		protected bool adjustViewDistanceToVehicleSize = false;

		[Tooltip("The default distance between the camera and the vehicle during vehicle selection.")]
		[SerializeField]
		protected float defaultVehicleViewDistance = 20;
	

		[Header("Module Mount Focus")]


		[Tooltip("The viewport position to place the module mount in during module selection.")]
		[SerializeField]
		protected Vector2 moduleViewportPosition = new Vector2(0.5f, 0.5f);

		[Tooltip("The distance from the module mount to place the camera during module selection.")]
		[SerializeField]
		protected float moduleMountViewDistance = 8;

		[Tooltip("The angle to view the module mount during module selection. This may be positive or negative based on the module mount position relative to the vehicle.")]
		[SerializeField]
		protected float maxModuleMountViewingAngle = 30;
	
		protected Vector3 positionTarget;
		protected Vector3 lookDirectionTarget;


		protected virtual void Reset()
		{
			loadoutManager = FindObjectOfType<LoadoutManager>();
			loadoutUIController = FindObjectOfType<LoadoutUIController>();
			displayManager = FindObjectOfType<LoadoutDisplayManager>();

			viewRotationBlockingLayers = LayerMask.GetMask("UI");
			viewRotationGimbal = GetComponentInChildren<GimbalController>();
		}

		protected virtual void Awake()
        {
			loadoutUIController.onVehicleSelectionMode.AddListener(OnVehicleSelectionMode);
			loadoutUIController.onModuleSelectionMode.AddListener(OnModuleSelectionMode);
        }


		protected virtual void OnVehicleSelectionMode()
        {
			viewRotationGimbal.HorizontalPivot.rotation = Quaternion.Euler(0, defaultVehicleFocusAngles.y, 0);
			viewRotationGimbal.VerticalPivot.rotation = Quaternion.Euler(defaultVehicleFocusAngles.x, 0, 0);
		}


		protected virtual void OnModuleSelectionMode()
        {

        }


		// Update the vehicle focus
		protected virtual void VehicleFocusUpdate()
		{
			if (loadoutManager.LoadoutData.SelectedSlot == null) return;
			if (loadoutManager.LoadoutData.SelectedSlot.selectedVehicleIndex == -1) return;

			Vehicle vehicle = displayManager.DisplayVehicles[loadoutManager.LoadoutData.SelectedSlot.selectedVehicleIndex];

			// Calculate the camera distance

			float vehicleFocusDistance = defaultVehicleViewDistance;
			if (adjustViewDistanceToVehicleSize && loadoutManager.LoadoutData.SelectedSlot != null && loadoutManager.LoadoutData.SelectedSlot.selectedVehicleIndex != -1)
			{
				vehicleFocusDistance = GetVehicleViewDistance(vehicle);
			}

			// Calculate the local position for the camera

			Vector3 targetLocalPosition = ViewportPositionAdjustment(vehicleFocusDistance, vehicleViewportPosition);
			m_Camera.transform.SetParent(vehicleFocusCameraHolder);
			vehicleFocusCameraHolder.localPosition = Vector3.forward * vehicleFocusDistance;

			// Position and rotate the camera

			if (snapPositionAndRotation)
			{
				m_Camera.transform.localPosition = targetLocalPosition;
				m_Camera.transform.localRotation = Quaternion.identity;
			}
			else
			{
				m_Camera.transform.localPosition = Vector3.Lerp(m_Camera.transform.localPosition, targetLocalPosition, moveSpeed * Time.deltaTime);
				m_Camera.transform.localRotation = Quaternion.Slerp(m_Camera.transform.rotation, Quaternion.identity, rotationSpeed * Time.deltaTime);
				m_Camera.transform.rotation = Quaternion.LookRotation(m_Camera.transform.forward, Vector3.up);	// Always keep the camera vertical
			}
		}


		// Update the module focus
		protected virtual void ModuleFocusUpdate()
        {
			// Check 
			if (loadoutManager.LoadoutData.SelectedSlot == null) return;
			if (loadoutManager.LoadoutData.SelectedSlot.selectedVehicleIndex == -1) return;
			if (loadoutManager.SelectedModuleMountIndex == -1) return;

			LoadoutVehicleItem loadoutVehicleItem = loadoutManager.Items.vehicles[loadoutManager.LoadoutData.SelectedSlot.selectedVehicleIndex];
			Vehicle vehicle = displayManager.DisplayVehicles[loadoutManager.LoadoutData.SelectedSlot.selectedVehicleIndex];

			ModuleMount moduleMount = vehicle.ModuleMounts[loadoutManager.SelectedModuleMountIndex];


			// Calculate the position target for the camera

			Vector3 offset = moduleMount.transform.position - vehicle.transform.TransformPoint(vehicle.Bounds.center + loadoutVehicleItem.moduleMountViewAlignmentOffset);
			float maxOffsetY = Mathf.Tan(maxModuleMountViewingAngle * Mathf.Deg2Rad) * offset.magnitude;
			offset.y = Mathf.Clamp(offset.y, -maxOffsetY, maxOffsetY);

			if (offset.magnitude < 0.0001f)
			{
				offset = Vector3.forward;
			}
			else
			{
				offset.Normalize();
			}

			offset *= moduleMountViewDistance;

			positionTarget = moduleMount.transform.position + offset;

			// Calculate the rotation target
			
			Vector3 lookDirectionTarget = moduleMount.transform.position - positionTarget;
			Quaternion rotationTarget = Quaternion.LookRotation(lookDirectionTarget, Vector3.up);

			// Calculate and apply the position offset to achieve the correct viewport position

			Vector3 orientationRight = Vector3.Cross(Vector3.up, lookDirectionTarget).normalized;
			Vector3 orientationUp = Vector3.Cross(lookDirectionTarget, orientationRight).normalized;
			Vector3 viewportPositionAdjustment = ViewportPositionAdjustment(GetModuleViewDistance(), moduleViewportPosition);

			positionTarget += orientationRight * viewportPositionAdjustment.x;
			positionTarget += orientationUp * viewportPositionAdjustment.y;

			m_Camera.transform.SetParent(null);

			// Position and rotate the camera

			if (snapPositionAndRotation)
			{
				m_Camera.transform.position = positionTarget;
				m_Camera.transform.rotation = Quaternion.LookRotation(lookDirectionTarget, Vector3.up);
			}
			else
			{
				m_Camera.transform.position = Vector3.Lerp(m_Camera.transform.position, positionTarget, moveSpeed * Time.deltaTime);
				m_Camera.transform.rotation = Quaternion.Slerp(m_Camera.transform.rotation, rotationTarget, rotationSpeed * Time.deltaTime);
				m_Camera.transform.rotation = Quaternion.LookRotation(m_Camera.transform.forward, Vector3.up);
			}
		}


		// Calculate the local position offset to achieve a specified viewport position for a specified distance.
		protected virtual Vector3 ViewportPositionAdjustment(float distance, Vector2 viewportPosition)
        {

			// Calculate the half dimensions of the viewport in world coordinated at the specified distance

			Vector2 viewportHalfDimensions = Vector2.zero;
			viewportHalfDimensions.x = distance * Mathf.Tan((Camera.VerticalToHorizontalFieldOfView(m_Camera.fieldOfView, m_Camera.aspect) / 2) * Mathf.Deg2Rad);
			viewportHalfDimensions.y = distance * Mathf.Tan((m_Camera.fieldOfView / 2) * Mathf.Deg2Rad);

			// Calculate the correct position offset for the viewport coordinates

			Vector3 offset = Vector3.zero;
			offset += -Vector3.right * ((viewportPosition.x - 0.5f) * (viewportHalfDimensions.x * 2));
			offset += -Vector3.up * ((viewportPosition.y - 0.5f) * (viewportHalfDimensions.y * 2));

			return offset;
		}


		// Calculate the vehicle view distance for the camera
		protected virtual float GetVehicleViewDistance(Vehicle vehicle)
        {
			float diameter = Mathf.Max(new float[] { vehicle.Bounds.size.x, vehicle.Bounds.size.y, vehicle.Bounds.size.z });

			// Get the smaller dimension of the screen for determining the angle used to calculate the distance
			// the camera has to be to achieve the max viewport size set in the inspector
			bool useHorizontalAngle = m_Camera.aspect < 1;
			float halfAngle;
			if (useHorizontalAngle)
			{
				float tmp = 0.5f / Mathf.Tan((m_Camera.fieldOfView / 2) * Mathf.Deg2Rad);
				halfAngle = Mathf.Atan((0.5f * m_Camera.aspect) / tmp) * Mathf.Rad2Deg;
			}
			else
			{
				halfAngle = m_Camera.fieldOfView / 2;
			}

			// Calculate the distance of the camera to the target vehicle to achieve the viewport size
			float distance = ((diameter / 2) / maxViewportVehicleDiameter) / Mathf.Tan(halfAngle * Mathf.Deg2Rad);

			return distance;
		}


		// Get the module view distance for the camera
		protected virtual float GetModuleViewDistance()
        {
			return moduleMountViewDistance;
        }


		// Whether the cursor is over a blocking layer (e.g. UI) to prevent unwanted view rotation
		protected virtual bool IsPointerOverBlockingLayer()
		{
			// Get event system raycast results
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			eventData.position = Input.mousePosition;
			List<RaycastResult> raycastResults = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventData, raycastResults);

			// Check if the raycast results are on a blocking layer
			for (int i = 0; i < raycastResults.Count; i++)
			{
				RaycastResult result = raycastResults[i];
				if (viewRotationBlockingLayers == (viewRotationBlockingLayers | (1 << result.gameObject.layer))) return true;
			}

			return false;
		}


		// Rotate the camera view
		protected virtual void RotateView()
		{
			if (viewRotationGimbal == null) return;

			if (!viewRotating || IsPointerOverBlockingLayer())
			{
				targetGimbalRotationInput = Vector2.zero;
			}

			if (loadoutUIController.State == LoadoutUIController.UIState.ModuleSelection) return;

			currentGimbalRotationInput = Vector2.Lerp(currentGimbalRotationInput, targetGimbalRotationInput, viewRotationLerpSpeed * Time.deltaTime);
			Vector2 rotation = viewRotationSpeed * currentGimbalRotationInput * Time.deltaTime;
			viewRotationGimbal.Rotate(rotation.x, rotation.y);

			// Reset the input

			targetGimbalRotationInput = Vector2.zero;
		}


		/// <summary>
		/// Enable view rotation (for input scripts).
		/// </summary>
		/// <param name="viewRotating">Whether view rotation is enabled.</param>
		public virtual void SetViewRotating(bool viewRotating)
        {
			this.viewRotating = viewRotating;
        }


		/// <summary>
		/// Set the view rotation inputs (for input scripts).
		/// </summary>
		/// <param name="inputValues">The input values (positive/negative for each rotation axis).</param>
		public virtual void SetViewRotationInputs(Vector3 inputValues)
        {
			targetGimbalRotationInput = inputValues;
        }


		// Called every frame
		protected virtual void Update()
        {
			if (loadoutUIController.State == LoadoutUIController.UIState.ModuleSelection)
            {
				ModuleFocusUpdate();
            }
            else
            {
				VehicleFocusUpdate();
			}

			RotateView();
		}
	}
}
