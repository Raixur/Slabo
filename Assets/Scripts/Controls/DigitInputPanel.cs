using System;
using AudioSDK;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class DigitInputPanel : MonoBehaviour
{
    [SerializeField] private string inputLabel = "";
    [SerializeField] private DigitDisplay display = null;

    [SerializeField] private DigitButton[] buttons = null;
    [SerializeField] private VRTK_Button resetButton = null;

    [SerializeField] private AudioObject buttonAudio = null;
    [SerializeField] private string buttonClickAudio = "";


    private string code;

    [UsedImplicitly]
    private void Awake()
    {
        buttons = GetComponentsInChildren<DigitButton>();
    }

    // Use this for initialization
    [UsedImplicitly]
    private void OnEnable()
    {
        SubscribeInputButton();
        SubscribeResetButton();
        SubscribeDisplay();
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        UnsubscribeInputButton();
        UnsubscribeResetButton();
        UnsubscribeDisplay();
    }

    public string Value { get { return display.Value; } }

    #region Events

    public event EventHandler Activated;

    protected virtual void OnActivated()
    {
        var handler = Activated;
        if (handler != null) handler(this, EventArgs.Empty);
    }

    #endregion

    #region Button subscription manager

    private void SubscribeInputButton()
    {
        foreach (var button in buttons)
            button.DigitButtonPush += OnDigitButtonPush;
    }

    private void UnsubscribeInputButton()
    {
        foreach (var button in buttons)
            button.DigitButtonPush -= OnDigitButtonPush;
    }

    private void SubscribeResetButton()
    {
        if (resetButton != null) resetButton.Pushed += OnResetButtonPushed;
    }

    private void UnsubscribeResetButton()
    {
        if (resetButton != null) resetButton.Pushed -= OnResetButtonPushed;
    }

    private void SubscribeDisplay()
    {
        if (display != null) display.InputFinished += OnDisplayInputFinished;
    }

    private void UnsubscribeDisplay()
    {
        if (display != null) display.InputFinished -= OnDisplayInputFinished;
    }

    #endregion


    private void OnDigitButtonPush(object o, DigitButtonEventArgs args)
    {
        buttonAudio.PlayAfter(buttonClickAudio);
        display.AddDigit(args.Digit);
    }

    private void OnResetButtonPushed(object o, Control3DEventArgs control3DEventArgs)
    {
        buttonAudio.PlayAfter(buttonClickAudio);
        display.ResetDisplay();
    }

    private void OnDisplayInputFinished(object sender, DisplayPanelEventArgs args)
    {
        if (code == args.Value)
            OnActivated();
    }
}
