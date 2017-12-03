using Assets.Scripts.Screamer.Transition;
using UnityEngine;
using VRTK;

public class FadeTransition : BaseActivateTransition
{
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float unfadeDuration = 0.5f;
    [SerializeField] private VRTK_HeadsetFade headsetFade = null;
    [SerializeField] private Color color = Color.black;

    protected override float HandleAppear()
    {
        headsetFade.Fade(color, fadeDuration);
        return fadeDuration;
    }

    protected override float HandleDisappear()
    {
        headsetFade.Unfade(unfadeDuration);
        return unfadeDuration;
    }
}
