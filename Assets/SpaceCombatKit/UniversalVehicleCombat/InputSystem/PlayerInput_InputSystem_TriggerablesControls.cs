using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Input script for controlling the steering and movement of a space fighter vehicle.
    /// </summary>
    public class PlayerInput_InputSystem_TriggerablesControls : VehicleInput
    {

        protected GeneralInputAsset input;

        protected TriggerablesManager triggerablesManager;

  
        protected override void Awake()
        {
            base.Awake();

            input = new GeneralInputAsset();


            // Fire primary
            input.WeaponControls.FirePrimary.performed += ctx => FirePrimary(true);
            input.WeaponControls.FirePrimary.canceled += ctx => FirePrimary(false);

            // Fire secondary
            input.WeaponControls.FireSecondary.performed += ctx => FireSecondary(true);
            input.WeaponControls.FireSecondary.canceled += ctx => FireSecondary(false);


        }

        void FirePrimary(bool started)
        {
            if (!CanRunInput()) return;

            if (started)
                triggerablesManager.StartTriggeringAtIndex(0);
            else
                triggerablesManager.StopTriggeringAtIndex(0);
        }

        void FireSecondary(bool started)
        {
            if (!CanRunInput()) return;

            if (started)
                triggerablesManager.StartTriggeringAtIndex(1);
            else
                triggerablesManager.StopTriggeringAtIndex(1);
        }


        private void OnEnable()
        {
            input.Enable();
        }

        private void OnDisable()
        {
            input.Disable();
        }

        bool CanRunInput()
        {
            return (initialized && inputEnabled && inputUpdateConditions.ConditionsMet);
        }

        /// <summary>
        /// Initialize this input script with a vehicle.
        /// </summary>
        /// <param name="vehicle">The new vehicle.</param>
        /// <returns>Whether the input script succeeded in initializing.</returns>
        protected override bool Initialize(Vehicle vehicle)
        {

            if (!base.Initialize(vehicle)) return false;

            triggerablesManager = vehicle.GetComponent<TriggerablesManager>();

            if (triggerablesManager == null)
            {
                if (debugInitialization)
                {
                    Debug.Log(GetType().Name + " component failed to initialize, TriggerablesManager component not found on vehicle.");
                }
                return false;
            }
       
            if (debugInitialization)
            {
                Debug.Log(GetType().Name + " component successfully initialized.");
            }

            return true;
        }
    }
}