using UnityEngine;
using VRTK;


public struct DigitButtonEventArgs
{
    public int Digit;
}

public delegate void DigitButtonEventHandler(object sender, DigitButtonEventArgs args);

public class DigitButton : VRTK_Button
{
    [SerializeField] private int digit;

    public event DigitButtonEventHandler DigitButtonPush;

    public override void OnPushed(Control3DEventArgs e)
    {
        if (DigitButtonPush != null) DigitButtonPush(this, new DigitButtonEventArgs { Digit = digit });
    }
}