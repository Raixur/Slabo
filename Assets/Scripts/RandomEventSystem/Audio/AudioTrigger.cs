using AudioSDK;
using UnityEngine;

public class AudioTrigger : ZoneTriggerable
{
    [SerializeField] private string audioId;
    private bool isTriggered;

    public override void TriggerEnter()
    {
        if(!isTriggered)
        {
            AudioController.PlayAmbienceSound(audioId);
            isTriggered = true;
        }
    }

    public override void TriggerExit()
    {
    }
}
