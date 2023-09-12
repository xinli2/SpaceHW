using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    [System.Serializable]
    public class TransformFollower
    {
        public Transform follower;
        public Transform target;
        public TransformRotationFollowType rotationFollowType;
        public Vector3 eulerRotationOffset;

        public virtual void FollowUpdate()
        {
            Vector3 followerForward;
            switch (rotationFollowType)
            {
                case TransformRotationFollowType.FullRotation:

                    follower.rotation = target.rotation * Quaternion.Euler(eulerRotationOffset);
                    break;

                case TransformRotationFollowType.HorizontalOnly:

                    followerForward = target.forward;
                    followerForward.y = 0;
                    followerForward.Normalize();
                    follower.rotation = Quaternion.LookRotation(followerForward, follower.up);
                    break;

                case TransformRotationFollowType.LookDirectionOnly:

                    followerForward = target.forward;
                    follower.rotation = Quaternion.LookRotation(followerForward, follower.up);
                    break;

            }
        }
    }
}

