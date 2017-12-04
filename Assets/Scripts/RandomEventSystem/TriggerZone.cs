using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private ZoneTriggerable triggerable = null;

    [UsedImplicitly]
    private void OnTriggerEnter(Collider other)
    {
        if (VRTK_PlayerObject.IsPlayerObject(other.gameObject))
            triggerable.TriggerEnter();
    }

    [UsedImplicitly]
    private void OnTriggerExit(Collider other)
    {
        if (VRTK_PlayerObject.IsPlayerObject(other.gameObject))
            triggerable.TriggerExit();
    }
}
