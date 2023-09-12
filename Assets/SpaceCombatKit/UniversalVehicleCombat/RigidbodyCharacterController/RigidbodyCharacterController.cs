using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

    [System.Serializable]
    public class OnCharacterLandedEventHandler : UnityEvent<Vector3> { }

    /// <summary>
    /// Rigidbody-based character controller.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyCharacterController : MonoBehaviour
    {

        [SerializeField]
        protected bool characterControllerEnabled = true;
        public virtual bool CharacterControllerEnabled
        {
            get { return characterControllerEnabled; }
            set { characterControllerEnabled = value; }
        }

        // The capsule collider for this character.
        protected CapsuleCollider capsuleCollider;

        // The rigidbody of the character
        protected Rigidbody m_Rigidbody;
        public Rigidbody Rigidbody { get { return m_Rigidbody; } }

        protected bool inputEnabled = true;

        [SerializeField]
        protected float groundedDrag = 0;

        [SerializeField]
        protected float airborneDrag = 0.5f;

        [Header("Movement")]

        [Tooltip("The magnitude of the velocity when the character is walking.")]
        [SerializeField]
        protected float walkSpeed = 20;
        public float WalkSpeed { get { return walkSpeed; } }

        [Tooltip("The magnitude of the velocity when the character is running.")]
        [SerializeField]
        protected float runSpeed = 35;
        public float RunSpeed { get { return runSpeed; } }

        // Whether the character is running
        protected bool running = false;
        public bool Running { get { return running; } }

        [Tooltip("Determines how smoothly the character stops when the movement input is released.")]
        [SerializeField]
        protected float movementSmoothing = 2;

        [Tooltip("A value that modifies the movement speed of the character (applied e.g. as a result of damage.")]
        [SerializeField]
        protected float movementModifier = 1;

        // The current movement inputs for the character
        protected Vector3 movementInputs = Vector3.zero;
        public Vector3 MovementInputs { get { return movementInputs; } }

        // The velocity that is applied to the rigidbody during FixedUpdate
        protected Vector3 velocity;

        // Whether the character is reversing
        protected bool reversing = false;
        public bool Reversing { get { return reversing; } }

        [Header("Turning")]

        [Tooltip("How fast the character rotates in response to input.")]
        [SerializeField]
        protected float turnSpeed = 1;

        [Tooltip("How smoothly the character stops rotating once the turning input is released.")]
        [SerializeField]
        protected float turnSmoothing = 0;

        // The current rotation inputs
        protected Vector3 rotationInputs;
        public Vector3 RotationInputs { get { return rotationInputs; } }

        // The angular velocity that is applied to the character in the physics step
        protected Vector3 angularVelocity;

        [Header("Jumping")]

        [Tooltip("The impulse force that is applied over a single frame when the character jumps to send them into the air.")]
        [SerializeField]
        protected float jumpForce = 1500;

        // Whether the character is jumping
        protected bool jumping = false;
        public bool Jumping { get { return jumping; } }

        [Tooltip("This many gravities will be added to the Physics gravity setting for this character.")]
        [SerializeField]
        protected float numGravityAddition = 5;

        [Header("Jetpack")]

        [Tooltip("The upward force that is applied to the rigidbody when the character is jetpacking.")]
        [SerializeField]
        protected float jetPackForceUp = 75;

        [Tooltip("The forward/backward/side force that is applied to the rigidbody when the character is jetpacking with movement on the horizontal plane.")]
        [SerializeField]
        protected float jetPackForceForward = 20;

        // Whether the character is currently jetpacking
        protected bool jetpacking = false;
        public bool Jetpacking { get { return jetpacking; } }

        [SerializeField]
        protected bool jetpackingEnabled = true;
        public bool JetpackingEnabled
        {
            get { return jetpackingEnabled; }
            set { jetpackingEnabled = value; }
        }

        [Header("Ground Check")]

        [SerializeField]
        protected bool startGrounded = true;

        [SerializeField]
        protected float startGroundedCheckDistance = 1000;

        [Tooltip("The layers of objects that the character can move around on.")]
        [SerializeField]
        protected LayerMask groundMask;
        public LayerMask GroundMask 
        { 
            get { return groundMask; }
            set { groundMask = value; }
        }


        [Tooltip("The distance under the character within which it will snap onto the ground. Prevents becoming airborne from ramps/small bumps.")]
        [SerializeField]
        protected float groundCheckDistance = 1.5f;
        protected float currentGroundCheckDistance;

        // Whether the character is currently on the ground (not airborne)
        protected bool grounded;
        public bool Grounded { get { return grounded; } }

        // The normal of the ground surface currently detected
        protected Vector3 groundNormal = Vector3.up;

        // The position where the ground has been detected.
        protected Vector3 groundedPosition;

        [Header("Events")]
        
        public OnCharacterLandedEventHandler onGrounded;

        public UnityEvent onAirborne;

        public UnityEvent onJump;

        public UnityEvent onJetpackStarted;

        public UnityEvent onJetpackStopped;

        public UnityEvent onJetpackActive;

        



        protected virtual void Awake()
        {
            // Get required components
            m_Rigidbody = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();

            // Initialize the current ground check distance
            currentGroundCheckDistance = groundCheckDistance;
        }


        protected virtual void Start()
        {
            if (startGrounded)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, -Vector3.up, out hit, startGroundedCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
                {
                    m_Rigidbody.position = hit.point;
                    m_Rigidbody.velocity = Vector3.zero;
                    SetGrounded(true);
                }
            }
        }


        // Called when this component is first added to a gameobject, or reset in the inspector.
        protected virtual void Reset()
        {
            // Initialize the rigidbody constraints to prevent rotation as a result of physics collisions
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            groundMask = ~0;

            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for(int i = 0; i < renderers.Length; ++i)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                bounds.center -= transform.position;

                float radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
                GetComponent<CapsuleCollider>().radius = radius;
                GetComponent<CapsuleCollider>().height = bounds.size.y;
                GetComponent<CapsuleCollider>().center = new Vector3(0, Mathf.Max(bounds.extents.y, radius), 0);
            }
            else
            {
                GetComponent<CapsuleCollider>().radius = 1.5f;
                GetComponent<CapsuleCollider>().height = 4;
                GetComponent<CapsuleCollider>().center = new Vector3(0, 2, 0);
            }
        }

         
        public virtual void SetInputEnabled(bool isEnabled)
        {
            inputEnabled = isEnabled;
            if (!inputEnabled)
            {
                ClearInputs();
            }
        }


        public virtual void ClearInputs()
        {
            movementInputs = Vector3.zero;
            rotationInputs = Vector3.zero;
            velocity = Vector3.zero;
        }


        // Called every time the user changes something in the inspector
        protected virtual void OnValidate()
        {
            // Prevent divide by zero errors by keeping smoothing values from becoming negative.
            movementSmoothing = Mathf.Max(movementSmoothing, 0);
            turnSmoothing = Mathf.Max(turnSmoothing, 0);
        }

        // Called every frame
        protected virtual void Update()
        {
            if (!characterControllerEnabled) return;

            // If on the downward end of a jump, switch back to a full-length ground check
            if (jumping)
            {
                if (m_Rigidbody.velocity.y < 0)
                {
                    currentGroundCheckDistance = groundCheckDistance;
                }
            }

            // If the character is grounded, stop jumping
            if (grounded)
            {
                jumping = false;
            }

            // Make sure jetpacking only works if it is enabled
            if (jetpacking)
            {
                jetpacking = jetpackingEnabled;
            }
        }

        /// <summary>
        /// Set the movement modifier to change the movement speed.
        /// </summary>
        /// <param name="newValue">The new movement modifier.</param>
        public void SetMovementModifier(float newValue)
        {
            movementModifier = newValue;
        }

        /// <summary>
        /// Set the movement inputs (-1 to 1 on each axis).
        /// </summary>
        /// <param name="movementInputs">The new movement input values.</param>
        public virtual void SetMovementInputs(Vector3 movementInputs)
        {
            if (inputEnabled)
            {
                this.movementInputs = movementInputs;
            }
        }

        /// <summary>
        /// Set the rotation inputs (-1 to 1 around each axis).
        /// </summary>
        /// <param name="rotationInputs">The new rotation input values.</param>
        public virtual void SetRotationInputs(Vector3 rotationInputs)
        {
            if (inputEnabled)
            {
                this.rotationInputs = rotationInputs;
            }
        }

        /// <summary>
        /// Make the character jump.
        /// </summary>
        public virtual void Jump()
        {
            if (!characterControllerEnabled || !inputEnabled || !CanJump()) return;

            SetGrounded(false);
            currentGroundCheckDistance = -1f;
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 0f, m_Rigidbody.velocity.z);
            m_Rigidbody.AddForce(Vector3.up * jumpForce);
            jumping = true;
        }

        // Check if the character can jump
        protected virtual bool CanJump()
        {
            return (characterControllerEnabled && !jumping && grounded);
        }

        /// <summary>
        /// Activate the character's jetpack.
        /// </summary>
        public virtual void ActivateJetpack()
        {
            if (!characterControllerEnabled) return;
            jetpacking = true;
            onJetpackStarted.Invoke();
        }

        /// <summary>
        /// Deactivate the character's jetpack.
        /// </summary>
        public virtual void DeactivateJetpack()
        {
            if (!characterControllerEnabled) return;
            jetpacking = false;
            onJetpackStopped.Invoke();
        }

        /// <summary>
        /// Set whether the character is running.
        /// </summary>
        /// <param name="running">Whether the character is running.</param>
        public virtual void SetRunning(bool running)
        {
            if (reversing)
            {
                this.running = false;
            }
            else
            {
                this.running = running;
            }
        }

        /// <summary>
        /// Set whether the character is reversing (facing away from movement direction).
        /// </summary>
        /// <param name="reversing">Whether the character is reversing.</param>
        public virtual void SetReversing(bool reversing)
        {
            this.reversing = reversing;
        }

        // Set whether the character is grounded or not
        protected virtual void SetGrounded(bool grounded)
        {
            // Become grounded
            if (grounded)
            {
                // If not already grounded, call the grounded event.
                if (!this.grounded)
                {
                    onGrounded.Invoke(m_Rigidbody.velocity);
                }
                
                this.grounded = true;
                jetpacking = false;
                m_Rigidbody.useGravity = false;
                m_Rigidbody.drag = groundedDrag;
                velocity.y = 0;
            }
            // Become airborne
            else
            {
                // If grounded, call the airborne event.
                if (this.grounded)
                {
                    onAirborne.Invoke();
                }

                this.grounded = false;
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 0f, m_Rigidbody.velocity.z);
                m_Rigidbody.useGravity = true;
                m_Rigidbody.drag = airborneDrag;
            }
        }

        // Check if the character is grounded
        protected virtual void CheckIsGrounded()
        {
            // Prepare the sphere cast parameters
            RaycastHit hit;
            Vector3 sphereCastStartPos = m_Rigidbody.position;
            float sphereCastStartOffset = 1f;
            Vector3 startingPos = sphereCastStartPos + Vector3.up * (capsuleCollider.radius + sphereCastStartOffset);
            float castDistance = sphereCastStartOffset + currentGroundCheckDistance;

            // Do a spherecast
            if (Physics.SphereCast(startingPos, capsuleCollider.radius, -Vector3.up, out hit, castDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                // Check if there is anything directly below the character, to prevent the character getting stuck on the edge of precipices.
                RaycastHit raycastGroundHit;
                if (!Physics.Raycast(startingPos, -Vector3.up, out raycastGroundHit, castDistance + capsuleCollider.radius, groundMask, QueryTriggerInteraction.Ignore))
                {
                    if (grounded)
                    {
                        // If no ground detected by raycast, allow to fall off the edge.
                        SetGrounded(false);
                    }
                }
                else
                {
                    // Find the ground contact position and snap the capsule collider onto it.
                    groundNormal = Vector3.ProjectOnPlane(hit.normal, transform.right).normalized;

                    groundedPosition = startingPos - Vector3.up * hit.distance - (Vector3.up * capsuleCollider.radius);   // hit.distance returns the amount the sphere travelled before hitting something

                    SetGrounded(true);
                }
            }
            // Airborne
            else
            {
                if (grounded)
                {
                    SetGrounded(false);
                }
            }
        }

        // Do movement when grounded
        protected virtual void GroundedMovement()
        {

            // Calculate velocity
            Vector3 nextVelocity = transform.TransformDirection(movementInputs) * (running ? runSpeed : walkSpeed);
            nextVelocity *= movementModifier;

            // Smooth the movement change
            velocity = Vector3.Lerp(velocity, nextVelocity, (1 / (1 + movementSmoothing)));

            // Project the velocity onto the ground plane while keeping the magnitude the same
            Vector3 velocityProjection = Vector3.ProjectOnPlane(velocity, groundNormal);
            velocity = velocityProjection.normalized * velocity.magnitude;

            // Position the rigidbody
            m_Rigidbody.position = groundedPosition;

            // Set the rigidbody velocity
            m_Rigidbody.velocity = velocity;

            // Rotate the rigidbody at the current angular velocity.
            Vector3 targetAngularVelocity = rotationInputs * turnSpeed;
            Vector3 nextAngularVelocity = Vector3.Lerp(angularVelocity, targetAngularVelocity, (1 / (1 + turnSmoothing)));

            Quaternion targetRotation = m_Rigidbody.rotation * Quaternion.Euler(nextAngularVelocity);

            m_Rigidbody.MoveRotation(targetRotation);

        }

        protected virtual void AirborneMovement()
        {
            // Add gravity
            m_Rigidbody.AddForce(Physics.gravity * numGravityAddition);

            // Jetpacking
            if (jetpacking && jetpackingEnabled)
            {
                m_Rigidbody.AddForce(new Vector3(0f, jetPackForceUp, 0));
                m_Rigidbody.AddForce(transform.TransformDirection(movementInputs) * jetPackForceForward);
            }

            // Rotation
            Vector3 targetAngularVelocity = rotationInputs * turnSpeed;
            Vector3 nextAngularVelocity = Vector3.Lerp(angularVelocity, targetAngularVelocity, (1 / (1 + turnSmoothing)));
            Quaternion targetRotation = m_Rigidbody.rotation * Quaternion.Euler(nextAngularVelocity);
            
            m_Rigidbody.MoveRotation(targetRotation);
        }


        protected virtual void FixedUpdate()
        {
            if (!characterControllerEnabled) return;

            // Check if the character is grounded
            CheckIsGrounded();

            // Handle grounded state
            if (grounded)
            {
                GroundedMovement();
            }
            // Handle airborne state
            else
            {
                AirborneMovement();
            }
            
            if (jetpacking)
            {
                onJetpackActive.Invoke();
            }

            // Update the stored velocity
            velocity = m_Rigidbody.velocity;
        }

        public float MaxFallSpeed
        {
            get 
            {
                float force = (numGravityAddition + 1) * Physics.gravity.magnitude;
                return (force / airborneDrag - force * Time.fixedDeltaTime) / m_Rigidbody.mass;
            }
        }

        public static float GetSpeedFromForce(float force, Rigidbody rBody)
        {
            return (force / rBody.drag - force * Time.fixedDeltaTime) / rBody.mass; // Subtracting (force * Time.fixedDeltaTime) / rBody.mass because drag is applied AFTER force is added
        }
    }
}

