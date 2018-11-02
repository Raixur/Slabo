using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class FadeTransition : BaseActivateTransition
{
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float unfadeDuration = 0.5f;
    [SerializeField] private Color color = Color.black;

    private VRTK_HeadsetFade headsetFade;

    [UsedImplicitly]
    private void Awake()
    {
        headsetFade = GetComponentInParent<VRTK_HeadsetFade>() ?? gameObject.AddComponent<VRTK_HeadsetFade>();
    }

    protected override float HandleAppear()
    {
        return Fade();
    }

    protected override float HandleDisappear()
    {
        return Fade();
    }

    private float Fade()
    {
        StartCoroutine(FadeCoroutine());
        return fadeDuration + unfadeDuration;
    }

    private IEnumerator FadeCoroutine()
    {
        headsetFade.Fade(color, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);
        headsetFade.Unfade(unfadeDuration);
    }
}
