using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    public class RigidbodyCharacterFootstepController : MonoBehaviour
    {

        [SerializeField]
        protected RigidbodyCharacterController m_RigidbodyCharacterController;

        [SerializeField]
        protected Transform leftFoot;

        [SerializeField]
        protected Transform rightFoot;

        [SerializeField]
        protected float footstepDownHeightThreshold;

        protected bool leftFootDown;
        protected bool rightFootDown;

        protected Foot nextFoot;

        protected enum Foot
        {
            Any,
            Left,
            Right
        }

        public UnityEvent onLeftFootDown;
        public UnityEvent onRightFootDown;


        protected void Reset()
        {
            m_RigidbodyCharacterController = GetComponent<RigidbodyCharacterController>();
        }

        protected virtual void Start()
        {
            leftFootDown = true;
            rightFootDown = true;
        }

        protected virtual void FootstepLeft()
        {
            onLeftFootDown.Invoke();
        }

        protected virtual void FootstepRight()
        {
            onRightFootDown.Invoke();
        }

        private void LateUpdate()
        {
            UpdateLegCheck();
        }

        public void UpdateLegCheck()
        {

            if (!m_RigidbodyCharacterController.Grounded) return;

            if (leftFoot == null || rightFoot == null) return;

            float walkAmount = Mathf.Min((m_RigidbodyCharacterController.Rigidbody.velocity.magnitude / m_RigidbodyCharacterController.WalkSpeed), 1);

            float runMargin = m_RigidbodyCharacterController.RunSpeed - m_RigidbodyCharacterController.WalkSpeed;
            float runAmount = (runMargin == 0 || m_RigidbodyCharacterController.Reversing) ? 0 : Mathf.Max(m_RigidbodyCharacterController.Rigidbody.velocity.magnitude - m_RigidbodyCharacterController.WalkSpeed, 0) / runMargin;

            float movement = walkAmount * 0.5f + runAmount * 0.5f;

            if (Mathf.Abs(movement) < 0.05f || !m_RigidbodyCharacterController.Grounded)
            {
                nextFoot = Foot.Any;
            }

            if (!leftFootDown)
            {
                if (m_RigidbodyCharacterController.transform.InverseTransformPoint(leftFoot.position).y <= footstepDownHeightThreshold)
                {
                    leftFootDown = true;
                    if (nextFoot == Foot.Any || nextFoot == Foot.Left)
                    {
                        FootstepLeft();
                        nextFoot = Foot.Right;
                    }
                }
            }
            else
            {
                if (m_RigidbodyCharacterController.transform.InverseTransformPoint(leftFoot.position).y > footstepDownHeightThreshold)
                {
                    leftFootDown = false;
                }
            }

            if (!rightFootDown)
            {
                if (m_RigidbodyCharacterController.transform.InverseTransformPoint(rightFoot.position).y <= footstepDownHeightThreshold)
                {
                    rightFootDown = true;
                    if (nextFoot == Foot.Any || nextFoot == Foot.Right)
                    {
                        FootstepRight();
                        nextFoot = Foot.Left;
                    }
                }
            }
            else
            {
                if (m_RigidbodyCharacterController.transform.InverseTransformPoint(rightFoot.position).y > footstepDownHeightThreshold)
                {
                    rightFootDown = false;
                }
            }
        }
    }

}
