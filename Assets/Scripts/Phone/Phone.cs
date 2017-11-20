using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;
using TMPro;

public class Phone : MonoBehaviour
{
    private const int RequiredDigits = 4;

    [SerializeField] private TextMeshPro display = null;
    [SerializeField] private List<VRTK_Button> buttons = null;
    [SerializeField] private VRTK_Button resetButton = null;
    [SerializeField] private VRTK_Button callButton = null;

    private List<int> digits = new List<int>(RequiredDigits);

    public void Start()
    {
        display.SetText("");
        for (int i = 0; i < buttons.Count; i++)
        {
            var number = i;
            buttons[i].Pushed += (sender, args) => InputNumber(number);
        }

        if(resetButton != null) resetButton.Pushed += (sender, args) => ResetNumbers();
        if(callButton != null) callButton.Pushed += (sender, args) => StartCall();
    }

    public void InputNumber(int number)
    {
        Debug.Log("pressed");
        if (digits.Count < RequiredDigits)
        {
            digits.Add(number);
            UpdateDisplay();
        }
    }

    public void ResetNumbers()
    {
        digits.Clear();
        display.SetText("");
    }

    public void StartCall()
    {
        Debug.Log("Start calling");
    }

    public void UpdateDisplay()
    {
        display.SetText(string.Join("", digits.Select(d => d.ToString()).ToArray()));
    }
}
