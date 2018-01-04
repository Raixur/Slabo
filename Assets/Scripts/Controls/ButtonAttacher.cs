using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class ButtonAttacher : MonoBehaviour
{
    [SerializeField] private DigitInputPanel panel = null;
    private string doorCode;

    [UsedImplicitly]
    private void Start()
    {
        doorCode = CodeGenerator.Instance.DoorCode.Aggregate("", (s, d) => s + d);
        panel.Display.InputFinished += TryAttach;
    }

    private void TryAttach(object sender, DisplayPanelEventArgs args)
    {
        if (doorCode == args.Value)
        {
            var buttons = GetComponentsInChildren<VRTK_Button>();
            foreach (var button in buttons)
            {
                var joint = button.gameObject.GetComponent<Joint>();
                joint.connectedBody = gameObject.GetComponent<Rigidbody>();
            }
        }
    }
}
