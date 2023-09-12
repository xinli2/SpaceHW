using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VSX.Effects
{
    public class InputSystemGamepadRumbles : MonoBehaviour
    {
        [SerializeField]
        protected float lowFrequencyMotorAmount = 1;

        [SerializeField]
        protected float highFrequencyMotorAmount = 1;

        protected virtual void Update()
        {
            if (RumbleManager.Instance != null)
            {
                if (Gamepad.current != null)
                {
                    Gamepad.current.SetMotorSpeeds(lowFrequencyMotorAmount * RumbleManager.Instance.CurrentLevel,
                                                    highFrequencyMotorAmount * RumbleManager.Instance.CurrentLevel);
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (Gamepad.current != null)
            {
                Gamepad.current.SetMotorSpeeds(0, 0);
            }
        }
    }
}
