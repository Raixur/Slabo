﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class DisplayPanelEventArgs
{
    public string Value;
}

public delegate void DisplayPanelEventHandler(object sender, DisplayPanelEventArgs args);

[RequireComponent(typeof(TextMeshPro))]
public class DigitDisplay : MonoBehaviour
{
    [SerializeField] private int requiredDigits = 4;

    private TextMeshPro display;
    private List<int> digits;

    [UsedImplicitly]
    private void Start()
    {
        display = GetComponent<TextMeshPro>();
        digits = new List<int>(requiredDigits);
        
        ResetDisplay();
	}

    public event DisplayPanelEventHandler InputFinished;

    public string Value { get { return display.text; } }

    public void AddDigit(int digit)
    {
        if (digits.Count < requiredDigits)
        {
            digits.Add(digit);
            display.text = digits.Aggregate("", (c, d) => c + d);
        }

        if (digits.Count == requiredDigits)
            OnInputFinished(new DisplayPanelEventArgs{ Value = display.text });
    }

    public void ResetDisplay()
    {
        digits.Clear();
        display.text = "";
    }

    protected virtual void OnInputFinished(DisplayPanelEventArgs args)
    {
        if (InputFinished != null)
            InputFinished(this, args);
    }
}
