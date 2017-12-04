using UnityEngine;

public abstract class ZoneTriggerable : MonoBehaviour
{
    public abstract void TriggerEnter();
    public abstract void TriggerExit();
}