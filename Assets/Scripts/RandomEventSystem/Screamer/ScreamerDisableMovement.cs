using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class ScreamerDisableMovement : ScreamerActionComponent
{
    private VRTK_TouchpadControl[] movementControl;

    [UsedImplicitly]
    private void Awake()
    {
        movementControl = FindObjectsOfType<VRTK_TouchpadControl>();
    }

    public override void TriggerAction(float time) { StartCoroutine(DisableMovementCoroutine(time)); }

    private IEnumerator DisableMovementCoroutine(float time)
    {
        foreach(var control in movementControl)
            control.enabled = false;

        yield return new WaitForSeconds(time);

        foreach(var control in movementControl)
            control.enabled = true;
    }
}
