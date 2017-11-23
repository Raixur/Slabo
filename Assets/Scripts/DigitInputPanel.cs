using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class DigitInputPanel : MonoBehaviour
{
    public DigitDisplay Display = null;

    [SerializeField] private List<VRTK_Button> buttons = null;
    [SerializeField] private VRTK_Button resetButton = null;
    
    // Use this for initialization
    [UsedImplicitly]
    private void Start ()
    {
        for (var i = 0; i < buttons.Count; i++)
        {
            var number = i;
            buttons[i].Pushed += (sender, args) => Display.AddDigit(number);
        }

        if (resetButton != null) resetButton.Pushed += (sender, args) => Display.ResetDisplay();
    }

    public string Value { get { return Display.Value; } }
}
