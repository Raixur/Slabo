using System.Collections;
using UnityEngine;

public class BaseActivateTransition : VisualTransition
{
    public override float Appear(Screamer screamer)
    {
        var transitionTime = HandleAppear();
        StartCoroutine(SetVisibilityCoroutine(screamer, transitionTime, true));

        return transitionTime;
    }
    
    public override float Disappear(Screamer screamer)
    {
        var transitionTime = HandleDisappear();
        StartCoroutine(SetVisibilityCoroutine(screamer, transitionTime, false));

        return transitionTime;
    }

    protected virtual float HandleAppear() { return 0f; }

    protected virtual float HandleDisappear() { return 0f; }

    private static IEnumerator SetVisibilityCoroutine(Screamer screamer, float time, bool visible)
    {
        yield return new WaitForSeconds(time);
        screamer.SetVisibility(visible);
    }
}