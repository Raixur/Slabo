using System;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class ButtonAttacher : MonoBehaviour
{
    [SerializeField] private DigitInputPanel panel = null;

    [UsedImplicitly]
    private void Start()
    {
        panel.Activated += Attach;
    }

    private void Attach(object sender, EventArgs args)
    {
        var buttons = GetComponentsInChildren<VRTK_Button>();
        foreach (var button in buttons)
        {
            var joint = button.gameObject.GetComponent<Joint>();
            joint.connectedBody = gameObject.GetComponent<Rigidbody>();
        }
    }
}
