using AudioSDK;
using UnityEngine;

public class AudioTrigger : MonoBehaviour, IZoneTriggerable
{
    [SerializeField] private string audioId;
    private bool isTriggered;

    public void TriggerEnter()
    {
        if(!isTriggered)
        {
            AudioController.PlayAmbienceSound(audioId);
            isTriggered = true;
        }
    }

    public void TriggerExit()
    {
    }
}
