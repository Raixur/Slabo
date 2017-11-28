using JetBrains.Annotations;
using UnityEngine;
using VRTK;

[RequireComponent(typeof(ScreamerSpawner))]
public class ScreamerDisableMovement : MonoBehaviour
{
    [SerializeField] private bool enableOnFacing = false;
    [SerializeField] private bool enableOnEnd = true;

    private VRTK_TouchpadControl[] movementControl;

    [UsedImplicitly]
    private void Start()
    {
        movementControl = FindObjectsOfType<VRTK_TouchpadControl>();

        var spawner = GetComponent<ScreamerSpawner>();
        spawner.Faced += (sender, args) => DisableMovement();

        if (enableOnFacing)
        {
            spawner.Unfaced += (sender, args) => EnableMovement();
        }

        if (enableOnEnd)
        {
            spawner.End += (sender, args) => EnableMovement();
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
