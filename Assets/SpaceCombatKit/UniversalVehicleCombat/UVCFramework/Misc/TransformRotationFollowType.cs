using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Describes the way that transforms can follow eachother in terms of rotation.
    /// </summary>
    public enum TransformRotationFollowType
    {
        FullRotation,
        HorizontalOnly,
        VerticalOnly,
        LookDirectionOnly
    }
}
