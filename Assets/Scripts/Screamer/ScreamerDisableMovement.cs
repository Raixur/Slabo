using System;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

[RequireComponent(typeof(ScreamerSpawner))]
public class ScreamerDisableMovement : MonoBehaviour
{
    private VRTK_TouchpadControl[] movementControl;

    [UsedImplicitly]
    private void Start()
    {
        movementControl = FindObjectsOfType<VRTK_TouchpadControl>();

        var spawner = GetComponent<ScreamerSpawner>();
        spawner.SpawnStart += OnFacing;
        spawner.DespawnEnd += OnUnfacing;
    }

    private void OnUnfacing(object sender, ScreamerEventArgs args)
    {
        var facingScreamer = args.Screamer as FacingScreamer;
        if (facingScreamer != null)
        {
            EnableMovement();
        }
    }

    private void OnFacing(object o, ScreamerEventArgs args)
    {
        var facingScreamer = args.Screamer as FacingScreamer;
        if (facingScreamer != null)
        {
            DisableMovement();
        }
    }

    private void DisableMovement()
    {
        foreach (var control in movementControl)
            control.enabled = false;
    }

    private void EnableMovement()
    {
        foreach (var control in movementControl)
            control.enabled = true;
    }
}
