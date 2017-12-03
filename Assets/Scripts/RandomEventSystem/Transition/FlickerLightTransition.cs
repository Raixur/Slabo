using Assets.Scripts.Screamer.Transition;
using UnityEngine;

public class FlickerLightTransition : BaseActivateTransition
{
    [SerializeField] private LightFlickering affectedLight = null;
    [SerializeField] private int flickeringCount = 1;

    protected override float HandleAppear() { return StartFlickering(); }

    protected override float HandleDisappear() { return StartFlickering(); }

    private float StartFlickering()
    {
        return affectedLight.Flicker(flickeringCount);
    }
}
