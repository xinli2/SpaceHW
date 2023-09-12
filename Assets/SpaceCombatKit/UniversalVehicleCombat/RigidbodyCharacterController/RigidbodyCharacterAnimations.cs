using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat.Mechs
{
    public class RigidbodyCharacterAnimations : MonoBehaviour
    {

        [SerializeField]
        protected RigidbodyCharacterController m_RigidbodyCharacterController;

        [Header("Animator State Settings")]

        [SerializeField]
        protected List<AnimatorStateSettings> animatorStateSettings = new List<AnimatorStateSettings>();
        
        [Header("Animator Parameters")]

        [SerializeField]
        protected List<AnimatorFloatParameterItem> forwardFloatParameterReceivers = new List<AnimatorFloatParameterItem>();

        [SerializeField]
        protected List<AnimatorFloatParameterItem> turnFloatParameterReceivers = new List<AnimatorFloatParameterItem>();

        [SerializeField]
        protected List<AnimatorBoolParameterItem> groundedBoolParameterReceivers = new List<AnimatorBoolParameterItem>();
       
        [SerializeField]
        protected List<AnimatorTriggerParameterItem> landedTriggerParameterReceivers = new List<AnimatorTriggerParameterItem>();

        [SerializeField]
        protected List<AnimatorTriggerParameterItem> jumpTriggerParameterReceivers = new List<AnimatorTriggerParameterItem>();

        [Header("Curve Animations")]

        [SerializeField]
        protected List<CurveAnimationsController> groundedMovementAnimations = new List<CurveAnimationsController>();

        [SerializeField]
        protected List<CurveAnimationsController> landedAnimations = new List<CurveAnimationsController>();        

        [Header("Grounded Movement")]
        
        protected float forwardValue;
        public float ForwardValue { get { return forwardValue; } }
        
        protected bool animationsEnabled = true;


        protected void Awake()
        {
            m_RigidbodyCharacterController.onJump.AddListener(OnJump);
            m_RigidbodyCharacterController.onGrounded.AddListener(OnGrounded);
            m_RigidbodyCharacterController.onAirborne.AddListener(OnAirborne);

            foreach(AnimatorFloatParameterItem forwardParameterReceiver in forwardFloatParameterReceivers)
            {
                forwardParameterReceiver.Initialize();
            }

            foreach (AnimatorFloatParameterItem turnParameterReceiver in turnFloatParameterReceivers)
            {
                turnParameterReceiver.Initialize();
            }

            foreach (AnimatorBoolParameterItem groundedBoolParameterReceiver in groundedBoolParameterReceivers)
            {
                groundedBoolParameterReceiver.Initialize();
            }

            foreach (AnimatorTriggerParameterItem landedTriggerParameterReceiver in landedTriggerParameterReceivers)
            {
                landedTriggerParameterReceiver.Initialize();
            }

            foreach (AnimatorTriggerParameterItem jumpTriggerParameterReceiver in jumpTriggerParameterReceivers)
            {
                jumpTriggerParameterReceiver.Initialize();
            }
        }

        protected virtual void Start()
        {
            if (m_RigidbodyCharacterController.Grounded)
            {
                OnGrounded(Vector3.zero);
            }
            else
            {
                OnAirborne();
            }
        }

        protected void Reset()
        {
            m_RigidbodyCharacterController = GetComponent<RigidbodyCharacterController>();

            Animator animatorController = GetComponentInChildren<Animator>();
            if (animatorController != null)
            {
                forwardFloatParameterReceivers.Add(new AnimatorFloatParameterItem(animatorController, "Forward"));
                turnFloatParameterReceivers.Add(new AnimatorFloatParameterItem(animatorController, "Turn"));
                groundedBoolParameterReceivers.Add(new AnimatorBoolParameterItem(animatorController, "Grounded"));
                landedTriggerParameterReceivers.Add(new AnimatorTriggerParameterItem(animatorController, "Landed"));

                animatorStateSettings.Add(new AnimatorStateSettings(animatorController, "Landing", true));
            }
        }

        public void SetAnimationsEnabled(bool animationsEnabled)
        {
            this.animationsEnabled = animationsEnabled;
        }

        void CheckAnimatorState()
        {
            for (int i = 0; i < animatorStateSettings.Count; ++i)
            {
                AnimatorStateInfo info = animatorStateSettings[i].animator.GetCurrentAnimatorStateInfo(0);

                if (info.fullPathHash != animatorStateSettings[i].lastAnimatorStateFullPathHash)
                {
                    bool found = false;
                    if (info.IsName(animatorStateSettings[i].stateName))
                    {
                        if (animatorStateSettings[i].disableCharacterController)
                        {
                            m_RigidbodyCharacterController.SetInputEnabled(false);
                        }
                        else
                        {
                            m_RigidbodyCharacterController.SetInputEnabled(true);
                        }

                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        m_RigidbodyCharacterController.SetInputEnabled(true);
                    }

                    animatorStateSettings[i].lastAnimatorStateFullPathHash = info.fullPathHash;
                }
            }
        }

        protected void Update()
        {
            float turnValue = 0;
            if (animationsEnabled)
            {
                // Forward value

                bool animateReversing = m_RigidbodyCharacterController.Reversing ? Vector3.Dot(transform.forward, transform.TransformDirection(m_RigidbodyCharacterController.MovementInputs.normalized)) < 0 : false;

                Vector3 forwardVelocity = Vector3.Dot(m_RigidbodyCharacterController.Rigidbody.velocity, m_RigidbodyCharacterController.Rigidbody.transform.forward) * m_RigidbodyCharacterController.Rigidbody.transform.forward;
                float walkAmount = Mathf.Min(forwardVelocity.magnitude / m_RigidbodyCharacterController.WalkSpeed, 1);

                float runMargin = m_RigidbodyCharacterController.RunSpeed - m_RigidbodyCharacterController.WalkSpeed;
                float runAmount = (runMargin == 0 || m_RigidbodyCharacterController.Reversing) ? 0 : Mathf.Max(forwardVelocity.magnitude - m_RigidbodyCharacterController.WalkSpeed, 0) / runMargin;

                forwardValue = walkAmount * 0.5f * (animateReversing ? -1 : 1) + runAmount * 0.5f;

                // Turn value

                turnValue = m_RigidbodyCharacterController.RotationInputs.y;

            }
            else
            {
                forwardValue = Mathf.Lerp(forwardValue, 0, 0.25f);
                turnValue = Mathf.Lerp(turnValue, 0, 0.25f);
            }

            // IMPORTANT - very small values cause blend trees to glitch out
            if (Mathf.Abs(turnValue) < 0.0001f) turnValue = 0;
            if (Mathf.Abs(forwardValue) < 0.0001f) forwardValue = 0;


            // Update forward parameter items
            for (int i = 0; i < forwardFloatParameterReceivers.Count; ++i)
            {
                forwardFloatParameterReceivers[i].SetParameter(forwardValue);
            }

            // Update float parameter items
            for (int i = 0; i < turnFloatParameterReceivers.Count; ++i)
            {
                turnFloatParameterReceivers[i].SetParameter(turnValue);
            }
            
            // Update forward curve animations
            if (m_RigidbodyCharacterController.Grounded)
            {
                for (int i = 0; i < groundedMovementAnimations.Count; ++i)
                {
                    groundedMovementAnimations[i].Animate(Mathf.Abs(forwardValue));
                }
            }

            CheckAnimatorState();
            
        }

        protected void OnGrounded(Vector3 landingVelocity)
        {

            // Update landed animator parameters
            for (int i = 0; i < landedTriggerParameterReceivers.Count; ++i)
            {
                landedTriggerParameterReceivers[i].SetParameter();
            }

            // Update grounded animator parameters
            for (int i = 0; i < groundedBoolParameterReceivers.Count; ++i)
            {
                groundedBoolParameterReceivers[i].SetParameter(true);
            }

            // Landed curve animations
            for (int i = 0; i < landedAnimations.Count; ++i)
            {
                landedAnimations[i].PlayOneShot();
            }
        }

        protected void OnJump()
        {
            // Update jump animator parameters
            for (int i = 0; i < jumpTriggerParameterReceivers.Count; ++i)
            {
                jumpTriggerParameterReceivers[i].SetParameter();
            }
        }

        protected void OnAirborne()
        {
            // Update grounded animator parameters
            for (int i = 0; i < groundedBoolParameterReceivers.Count; ++i)
            {
                groundedBoolParameterReceivers[i].SetParameter(false);
            }
        }
    }
}

