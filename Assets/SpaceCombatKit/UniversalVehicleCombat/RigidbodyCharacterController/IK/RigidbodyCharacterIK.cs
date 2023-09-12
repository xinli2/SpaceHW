using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;

public class RigidbodyCharacterIK : MonoBehaviour
{
    [Header("Foot IK")]

    [Tooltip("The rigidbody character controller.")]
    [SerializeField]
    protected RigidbodyCharacterController characterController;

    [Tooltip("The object that is moved to position the character so that the feet can touch the ground properly at all times.")]
    [SerializeField]
    protected Transform characterHandle;

    [Tooltip("The legs of this character for IK.")]
    [SerializeField]
    protected List<IKLeg> legs = new List<IKLeg>();

    [Tooltip("How fast the foot rotation changes based on the surface angle.")]
    [SerializeField]
    protected float footRotationLerpSpeed = 20;


    protected void Reset()
    {
        characterController = GetComponentInChildren<RigidbodyCharacterController>();

        Animator animatorController = GetComponentInChildren<Animator>();
        if (animatorController != null)
        {
            characterHandle = animatorController.transform;
        }
        
    }


    public static void DoLimbIK(IKJoint upperLimb, IKJoint lowerLimb, IKJoint endEffector, Vector3 endEffectorTargetPosition, Vector3 jointTargetPosition, float minExtension = 0, float maxExtension = 1)
    {

        float upperLimbLength = (lowerLimb.m_Transform.position - upperLimb.m_Transform.position).magnitude;
        float lowerLimbLength = (endEffector.m_Transform.position - lowerLimb.m_Transform.position).magnitude;
        float minExtensionLength = (upperLimbLength + lowerLimbLength) * minExtension;
        float maxExtensionLength = (upperLimbLength + lowerLimbLength) * maxExtension;

        // Clamp the length
        Vector3 offset = endEffectorTargetPosition - upperLimb.m_Transform.position;
        Vector3.ClampMagnitude(offset, maxExtensionLength);
        endEffectorTargetPosition = upperLimb.m_Transform.position + offset;

        float upperLimbToTargetLength = Mathf.Clamp((endEffectorTargetPosition - upperLimb.m_Transform.position).magnitude, minExtensionLength, maxExtensionLength);

        // Get the upper limb angle
        float upperLimbAngle = Mathf.Acos(((upperLimbLength * upperLimbLength) + (upperLimbToTargetLength * upperLimbToTargetLength) - (lowerLimbLength * lowerLimbLength)) /
                                            (2 * upperLimbLength * upperLimbToTargetLength)) * Mathf.Rad2Deg;

        // Point the upper limb at the target, oriented to the joint target
        upperLimb.m_Transform.LookAt(endEffectorTargetPosition, (jointTargetPosition - upperLimb.m_Transform.position).normalized);
        upperLimb.m_Transform.rotation = upperLimb.m_Transform.rotation * Quaternion.Euler(upperLimb.rotationOffset);

        // Get the rotation axis for the upper limb
        Vector3 upperLimbRotationAxis = Vector3.Cross(endEffectorTargetPosition - upperLimb.m_Transform.position, jointTargetPosition - upperLimb.m_Transform.position);

        // Rotate the upper limb out toward the joint target at the desired angle
        upperLimb.m_Transform.Rotate(upperLimbRotationAxis, upperLimbAngle, Space.World);

        // Rotate the lower limb back to the target position
        lowerLimb.m_Transform.LookAt(endEffectorTargetPosition, (jointTargetPosition - upperLimb.m_Transform.position).normalized);
        lowerLimb.m_Transform.rotation = lowerLimb.m_Transform.rotation * Quaternion.Euler(lowerLimb.rotationOffset);

    }


    protected virtual void UpdateCharacterPosition()
    {
        // The drop limit is the limit on how far the mech can be lowered such that any of its legs at the minimum extension are not embedded in the surface.
        float dropLimit = 0;
        for (int i = 0; i < legs.Count; ++i)
        {
            dropLimit = Mathf.Max(dropLimit, legs[i].GetLength());
        }

        // The drop is how much the mech will be lowered (may be negative/raised) to obtain the optimal extensions for the legs on an angled surface
        float drop = 0;

        // Remove previous mech offset while calculating the new one
        characterHandle.localPosition = Vector3.zero;

        // Only perform the drop if the mech is grounded
        if (characterController.Grounded)
        {
            for (int i = 0; i < legs.Count; ++i)
            {
                // The ray will be cast from the vertical position of the top of the limb, from a position directly above the foot.
                RaycastHit hit;
                float footToHipHeight = (legs[i].upperLeg.m_Transform.position - legs[i].foot.m_Transform.position).y;
                Vector3 raycastPos = legs[i].foot.m_Transform.position + Vector3.up * footToHipHeight;

                // Calculate the retracted length of the leg to update the drop limit
                float retractedLegLength = legs[i].minExtension * legs[i].GetLength() + legs[i].footHeight;

                // Calculate the optimum leg extension for updating the drop amount
                float targetLegLength = legs[i].upperLeg.m_Transform.position.y - characterController.transform.position.y;

                // Do raycast
                if (Physics.Raycast(raycastPos, -Vector3.up, out hit, legs[i].GetLength() * 2, characterController.GroundMask))
                {
                    // diff = How much does the mech need to drop to touch the ground with the target extension for that leg?
                    float diff = Mathf.Max((hit.point - raycastPos).magnitude - targetLegLength, 0);

                    // Update the drop
                    drop = Mathf.Max(drop, diff);

                    // Update the drop limit
                    dropLimit = Mathf.Min(dropLimit, (hit.point - raycastPos).magnitude - retractedLegLength);
                }
            }
        }

        // Set the drop within the drop limit
        drop = Mathf.Min(drop, dropLimit);

        // Position the mech
        if (characterHandle != null)
        {
            characterHandle.localPosition = new Vector3(0, -drop, 0);
        }
    }

    protected virtual void UpdateCharacterPlacement()
    {
        UpdateCharacterPosition();

        for (int i = 0; i < legs.Count; ++i)
        {
            // Get the surface target for the foot
            Vector3 hitPoint, hitNormal;
            if (GetFootSurfaceTarget(legs[i], out hitPoint, out hitNormal))
            {
                // Calculate the ankle target by offsetting the hit point by the foot height
                Vector3 ankleTarget = hitPoint + hitNormal * legs[i].footHeight;

                // Raise the ankle target proportionally to how far above the ground the animation had positioned the foot already
                float heightAmount = Mathf.Max((legs[i].foot.m_Transform.position.y - legs[i].footHeight) - characterHandle.position.y, 0) /
                                                (legs[i].upperLeg.m_Transform.position.y - characterHandle.position.y);

                float thisFrameHeight = Mathf.Max(legs[i].upperLeg.m_Transform.position.y - (ankleTarget.y - legs[i].footHeight), 0);
                ankleTarget.y += heightAmount * thisFrameHeight;

                // Calculate the joint target for the leg
                Vector3 jointTarget = legs[i].upperLeg.m_Transform.position + characterController.transform.TransformDirection(legs[i].relativeJointTarget);

                // Perform IK positioning of the leg
                DoLimbIK(legs[i].upperLeg, legs[i].lowerLeg, legs[i].foot, ankleTarget, jointTarget);

                // Calculate the foot rotation and lerp toward it
                //Quaternion thisFootRotation = Quaternion.LookRotation(hitPoint - legs[i].foot.m_Transform.position, characterController.transform.forward) * Quaternion.Euler(-90, 0, 0);
                Quaternion thisFootRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(characterController.transform.forward, hitNormal), hitNormal);
                thisFootRotation = thisFootRotation * Quaternion.Euler(legs[i].foot.rotationOffset);
                legs[i].foot.m_Transform.rotation = Quaternion.Slerp(legs[i].footRotation, thisFootRotation, footRotationLerpSpeed * Time.deltaTime);
                legs[i].footRotation = legs[i].foot.m_Transform.rotation;
            }
            else
            {
                // If the mech is not on the ground, lerp the foot rotation smoothly back to the animation rotation.
                legs[i].foot.m_Transform.rotation = Quaternion.Slerp(legs[i].footRotation, legs[i].foot.m_Transform.rotation, footRotationLerpSpeed * Time.deltaTime);
                legs[i].footRotation = legs[i].foot.m_Transform.rotation;
            }
        }
    }

    // Get the foot surface target
    protected virtual bool GetFootSurfaceTarget(IKLeg leg, out Vector3 position, out Vector3 normal)
    {
        // The ray will be cast from a position directly above the foot, at the level of the top of the limb
        Vector3 raycastPos = leg.foot.m_Transform.position;
        raycastPos.y = leg.upperLeg.m_Transform.position.y;

        // Perform the raycast
        RaycastHit hit;
        if (Physics.Raycast(raycastPos, -Vector3.up, out hit, leg.GetLength() * 2, characterController.GroundMask))
        {
            position = hit.point;
            normal = hit.normal;
            return true;
        }
        else
        {
            position = Vector3.zero;
            normal = Vector3.zero;
            return false;
        }
    }

    private void LateUpdate()
    {
        UpdateCharacterPlacement();
    }
}
