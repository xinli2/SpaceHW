using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;
using System.Linq;

namespace VSX.UniversalVehicleCombat.Mechs
{
    public class RigidbodyCharacterInput : VehicleInput
    {

        [Header("Settings")]

        [Tooltip("The rigidbody character controller that this input will be sent to.")]
        [SerializeField]
        protected RigidbodyCharacterController m_RigidbodyCharacterController;

        [Tooltip("The controller of the character torso gimbal, used to determine where the movement direction based on input relative to where the character is facing.")]
        [SerializeField]
        protected GimballedVehicleController lookController;
        
        [SerializeField]
        protected float movementSmoothing;
        
        protected Vector3 lastMovementInputs;
        

        // Called every time the user modifies a value in the inspector
        protected void OnValidate()
        {
            // Make sure smoothing is not negative to prevent divide-by-zero errors.
            movementSmoothing = Mathf.Max(movementSmoothing, 0);
        }
        

        // Called when this input script is disabled, e.g. when the game agent exits a vehicle
        public override void DisableInput()
        {
            base.DisableInput();

            if (m_RigidbodyCharacterController != null)
            {
                // Stop moving and rotating the character
                m_RigidbodyCharacterController.SetMovementInputs(Vector3.zero);
                m_RigidbodyCharacterController.SetRotationInputs(Vector3.zero);
            }
        }

        
        // Called when the game agent enters a vehicle
        protected override bool Initialize(Vehicle vehicle)
        {
            if (!base.Initialize(vehicle)) return false;

            // Grab necessary components
            m_RigidbodyCharacterController = vehicle.GetComponent<RigidbodyCharacterController>();
            lookController = vehicle.GetComponent<GimballedVehicleController>();

            return (m_RigidbodyCharacterController != null);
        }


        // Called every frame that this input script is active.
        protected override void InputUpdate()
        {
            
            // Get the movement input
            float forward = Input.GetAxis("Vertical");
            float right = Input.GetAxis("Horizontal");

            // Get the target input vector
            Vector3 movementInputs = Vector3.ClampMagnitude(new Vector3(right, 0f, forward), 1);

            // Get the next input vector
            movementInputs = Vector3.Lerp(lastMovementInputs, movementInputs, (1 / (1 + movementSmoothing)));
            lastMovementInputs = movementInputs;

            // Update whether the character is reversing
            bool reversing = movementInputs.z < -0.01f;
            m_RigidbodyCharacterController.SetReversing(reversing);

            // Convert the input to a local movement vector
            Vector3 worldMovementDirection;
            if (lookController != null)
            {
                worldMovementDirection = lookController.GimbalController.HorizontalPivot.TransformDirection(movementInputs).normalized;
            }
            else
            {
                worldMovementDirection = Camera.main.transform.TransformDirection(movementInputs).normalized;
            }

            Vector3 localMovementDirection = m_RigidbodyCharacterController.transform.InverseTransformDirection(worldMovementDirection);

            // Send movement inputs
            m_RigidbodyCharacterController.SetMovementInputs(localMovementDirection * movementInputs.magnitude);

            // Get the jump input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (m_RigidbodyCharacterController.Grounded)
                {
                    m_RigidbodyCharacterController.Jump();
                }
                else
                {
                    m_RigidbodyCharacterController.ActivateJetpack();
                }
            }

            // Deactivate jetpack on release of input
            if (Input.GetKeyUp(KeyCode.Space) && m_RigidbodyCharacterController.Jetpacking)
            {
                m_RigidbodyCharacterController.DeactivateJetpack();
            }

            // Set whether the character is running
            m_RigidbodyCharacterController.SetRunning(Input.GetKey(KeyCode.LeftShift));

            // Rotate the character to face the movement vector
            float turnAmount = 0;
            if (movementInputs.magnitude > 0.01f)
            {
                Vector3 localLegsTargetDirection = localMovementDirection;

                if (reversing)
                {
                    localLegsTargetDirection *= -1;
                }
                
                turnAmount = Mathf.Atan2(localLegsTargetDirection.x, localLegsTargetDirection.z) * 10;
            }
            
            // Send rotation inputs to the character
            m_RigidbodyCharacterController.SetRotationInputs(new Vector3(0f, turnAmount, 0f));

        }
    }
}

