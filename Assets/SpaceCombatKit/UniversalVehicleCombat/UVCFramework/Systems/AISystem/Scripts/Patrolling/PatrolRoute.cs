using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class represents a patrol route - a sequential, loopable series of patrol points that AI travels between
    /// </summary>
    public class PatrolRoute : MonoBehaviour 
	{
	    [Tooltip("The waypoints along the patrol route, will be followed in the list order.")]
		[SerializeField]
		protected List<Transform> waypoints = new List<Transform>();

        /// <summary>
        /// A list of all the patrol targets (nodes) along this route.
        /// </summary>
		public List<Transform> Waypoints { get { return waypoints; } }


        /// <summary>
        /// Get the length of the patrol route
        /// </summary>
        /// <param name="looping">Whether the length should include the return to the first waypoint.</param>
        /// <returns>The length of the patrol route.</returns>
        public virtual float GetLength(bool looping = true)
        {
            if (waypoints.Count < 2) return 0;

            float totalDistance = 0;
            for (int i = 1; i < waypoints.Count; ++i)
            {
                totalDistance += (waypoints[i].position - waypoints[i - 1].position).magnitude;
            }

            if (looping)
            {
                totalDistance += (waypoints[0].position - waypoints[waypoints.Count - 1].position).magnitude;
            }

            return totalDistance;
        }


        /// <summary>
        /// Get the world position of a normalized position value (0-1, where 0 is start, 1 is end) along the patrol route.
        /// </summary>
        /// <param name="normalizedPosition">The normalized route position (0 is start, 1 is end).</param>
        /// <param name="startWaypointIndex">The waypoint before the position.</param>
        /// <param name="endWaypointIndex">The waypoint after the position.</param>
        /// <param name="looping">Whether the normalized position includes a return to the first waypoint.</param>
        /// <returns>The world position on the route.</returns>
        public virtual Vector3 GetPositionOnRoute(float normalizedPosition, out int startWaypointIndex, out int endWaypointIndex, bool looping = true)
        {
            startWaypointIndex = -1;
            endWaypointIndex = -1;

            if (waypoints.Count < 2)
            {
                if (waypoints.Count == 1)
                {
                    return waypoints[0].position;
                }
                else
                {
                    return transform.position;
                }
            }

            startWaypointIndex = 0;
            endWaypointIndex = 1;

            float routeLength = GetLength(looping);

            float runningLength = 0;
            float diff = 0;
            for (int i = 1; i < waypoints.Count + 1; ++i)   // +1 for looping
            {
                runningLength += (waypoints[(i % waypoints.Count)].position - waypoints[(i - 1) % waypoints.Count].position).magnitude;
                if (runningLength >= normalizedPosition * routeLength)   // position is on this leg
                {
                    startWaypointIndex = (i - 1) % waypoints.Count;
                    endWaypointIndex = i % waypoints.Count;
                    diff = normalizedPosition - runningLength;
                    break;
                }
            }

            return (waypoints[startWaypointIndex].position + (waypoints[endWaypointIndex].position - waypoints[startWaypointIndex].position) * diff);
        }


        /// <summary>
        /// Get the world position of a random point along the patrol route.
        /// </summary>
        /// <param name="startWaypointIndex">The waypoint before the position.</param>
        /// <param name="endWaypointIndex">The waypoint after the position.</param>
        /// <param name="looping">Whether the normalized position includes a return to the first waypoint.</param>
        /// <returns>The world position on the route.</returns>
        public virtual Vector3 GetRandomPositionOnRoute(out int startWaypointIndex, out int endWaypointIndex, bool looping = true)
        {
            return GetPositionOnRoute(Random.Range(0f, 1f), out startWaypointIndex, out endWaypointIndex, looping);
        }
    }
}
