using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class TriggerZone : MonoBehaviour
{
    private List<ZoneTriggerable> triggerables;

    [UsedImplicitly]
    private void Awake()
    {
        triggerables = new List<ZoneTriggerable>(GetComponentsInChildren<ZoneTriggerable>());
    }

    [UsedImplicitly]
    private void OnTriggerEnter(Collider other)
    {
        if (VRTK_PlayerObject.IsPlayerObject(other.gameObject))
            triggerables.ForEach(t => t.TriggerEnter());
    }

    [UsedImplicitly]
    private void OnTriggerExit(Collider other)
    {
        if (VRTK_PlayerObject.IsPlayerObject(other.gameObject))
            triggerables.ForEach(t => t.TriggerExit());
    }
}
