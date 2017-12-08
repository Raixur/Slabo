using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class FlickerLightTransition : BaseActivateTransition
{
    [SerializeField] private int flickeringCount = 1;
    private List<LightFlickering> affectedLights;

    [UsedImplicitly]
    private void Awake()
    {
        affectedLights = new List<LightFlickering>(Resources.FindObjectsOfTypeAll<LightFlickering>());
    }

    protected override float HandleAppear() { return StartFlickering(); }

    protected override float HandleDisappear() { return StartFlickering(); }

    private float StartFlickering()
    {
        return affectedLights.Min(f => f.Flicker(flickeringCount));
    }
}
