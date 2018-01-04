using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class CodeDoor : MonoBehaviour
{
    [SerializeField] private DigitInputPanel panel = null;
    [SerializeField] private LockableDoor door = null;
    [SerializeField] private LockBolts bolts = null;

    private string doorCode;

    [UsedImplicitly]
    private void Start ()
	{
	    doorCode = CodeGenerator.Instance.DoorCode.Aggregate("", (s, d) => s + d);
        Debug.Log(doorCode);
        panel.Display.InputFinished += TryOpen;
	}

    private void TryOpen(object sender, DisplayPanelEventArgs args)
    {
        if (doorCode == args.Value)
        {
            door.SetOpen(true);
            panel.Display.InputFinished -= TryOpen;
            bolts.ToggleLock();
        }
    }
}
