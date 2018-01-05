using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class CodeDoor : MonoBehaviour
{
    [SerializeField] private DigitInputPanel panel = null;
    [SerializeField] private LockableDoor door = null;
    [SerializeField] private LockBolts bolts = null;

    [UsedImplicitly]
    private void Start ()
	{
        panel.Activated += Open;
	}

    private void Open(object sender, EventArgs args)
    {
        door.SetOpen(true);
        bolts.SetOpen(true);
    }
}
