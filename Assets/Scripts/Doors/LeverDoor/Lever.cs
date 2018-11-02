using System;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

[RequireComponent(typeof(VRTK_Lever))]
public class Lever : MonoBehaviour
{
    [SerializeField] private float normalizedValueSnap = 5f;

    private bool state = false;

    public event EventHandler TurnOn;
    public event EventHandler TurnOff;
    public event EventHandler Toggled;

    [UsedImplicitly]
    private void Awake()
    {
        var lever = GetComponent<VRTK_Lever>();
        lever.ValueChanged += LeverOnValueChanged;
    }

    private void LeverOnValueChanged(object sender, Control3DEventArgs control3DEventArgs)
    {
        if (state && control3DEventArgs.normalizedValue < normalizedValueSnap)
        {
            state = false;
            OnTurnOff();
            OnToggled();
        }

        if (!state && 100 - control3DEventArgs.normalizedValue < normalizedValueSnap)
        {
            state = true;
            OnTurnOn();
            OnToggled();
        }
    }

    protected virtual void OnTurnOn()
    {
        if (TurnOn != null)
            TurnOn(this, EventArgs.Empty);
    }

    protected virtual void OnTurnOff()
    {
        if (TurnOff != null)
            TurnOff(this, EventArgs.Empty);
    }

    protected virtual void OnToggled()
    {
        if (Toggled != null)
            Toggled(this, EventArgs.Empty);
    }
}
