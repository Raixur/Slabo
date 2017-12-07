using UnityEngine;
using VRTK;

[RequireComponent(typeof(VRTK_ControllerEvents))]
public class IndexFingerTouch : VRTK_InteractTouch
{
    public void Start()
    {
        var controller = GetComponent<VRTK_ControllerEvents>();
        controller.TriggerTouchEnd += EnableFinger;
        controller.TriggerTouchStart += DisableFinger;
    }

    private void EnableFinger(object sender, ControllerInteractionEventArgs e)
    {
        customColliderContainer.SetActive(true);
    }

    private void DisableFinger(object sender, ControllerInteractionEventArgs e)
    {
        ForceStopTouching();
        customColliderContainer.SetActive(false);
    }
}
