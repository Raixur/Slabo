using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private IZoneTriggerable screamer = null;

    [UsedImplicitly]
    private void OnTriggerEnter(Collider other)
    {
        if (VRTK_PlayerObject.IsPlayerObject(other.gameObject))
            screamer.TriggerEnter();
    }

    [UsedImplicitly]
    private void OnTriggerExit(Collider other)
    {
        if (VRTK_PlayerObject.IsPlayerObject(other.gameObject))
            screamer.TriggerExit();
    }
}
