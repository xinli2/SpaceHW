using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnCooldownValueChangedEventHandler : UnityEvent<float> { }

public class CooldownTimer : MonoBehaviour
{
    protected float currentValue;
    public float CurrentValue
    {
        get { return currentValue; }
    }

    [SerializeField]
    protected float cooldownRate = 5;

    protected bool isCooling = false;
    public bool IsCooling
    {
        get { return isCooling; }
    }

    [Header("Events")]

    public UnityEvent onCooldownStarted;

    public UnityEvent onCooldownFinished;

    public OnCooldownValueChangedEventHandler onCooldownValueChanged;

    [SerializeField]
    protected bool cooldownEnabled = true;
    public bool CooldownEnabled
    {
        get
        {
            return cooldownEnabled;
        }
        set
        {
            cooldownEnabled = value;
        }
    }

    public void SetValue(float newValue)
    {
        currentValue = Mathf.Clamp(newValue, 0, 1);
        if (currentValue > 0)
        {
            StartCooling();
        }
    }

    protected void StartCooling()
    {
        isCooling = true;
        onCooldownStarted.Invoke();
        onCooldownValueChanged.Invoke(currentValue);
    }

    protected void FinishCooling()
    {
        isCooling = false;
        currentValue = 0;
        onCooldownValueChanged.Invoke(currentValue);
        onCooldownFinished.Invoke();
    }

    private void Update()
    {
        if (isCooling && cooldownEnabled)
        {
            currentValue -= cooldownRate * Time.deltaTime;
            if (currentValue <= 0)
            {
                FinishCooling();
            }
            else
            {
                onCooldownValueChanged.Invoke(currentValue);
            }
        }
    }
}
