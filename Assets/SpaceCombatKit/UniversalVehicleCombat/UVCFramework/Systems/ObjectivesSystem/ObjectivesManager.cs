using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.ObjectivesSystem
{
    /// <summary>
    /// Manages all the objectives that make up the current mission.
    /// </summary>
    public class ObjectivesManager : MonoBehaviour
    {

        [Tooltip("List of components that each control a single objective.")]
        [SerializeField] protected List<ObjectiveController> objectiveControllers = new List<ObjectiveController>();

        [Tooltip("Event called when all the objectives are completed.")]
        public UnityEvent onObjectivesCompleted;


        protected virtual void Awake()
        {
            foreach (ObjectiveController objectiveController in objectiveControllers)
            {
                objectiveController.onCompleted.AddListener(CheckCompleted);
            }
        }


        // Check if all the objectives are completed
        protected virtual void CheckCompleted()
        {
            foreach (ObjectiveController objectiveController in objectiveControllers)
            {
                if (!objectiveController.Completed) return;
            }

            onObjectivesCompleted.Invoke();
        }
    }
}
