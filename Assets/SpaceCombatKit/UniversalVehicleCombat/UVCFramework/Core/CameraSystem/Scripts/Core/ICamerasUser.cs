using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Interface for a component that wishes to receive a list of cameras. Used to send list of following cameras to components on a vehicle.
    /// </summary>
    public interface ICamerasUser
    {
        void SetCameras(List<Camera> cameras);
    }
}

