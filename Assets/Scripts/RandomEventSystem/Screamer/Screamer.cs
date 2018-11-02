using AudioSDK;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Screamer : MonoBehaviour
{
    [SerializeField] private string triggerName = "";
    [SerializeField] private string audioId = "";

    private Animator animator;
    private bool hasAnimation;
    private bool hasAudio;

    [UsedImplicitly]
    private void Awake()
    {
        animator = GetComponent<Animator>();
        hasAnimation = !string.IsNullOrEmpty(triggerName);
        hasAudio = !string.IsNullOrEmpty(audioId);
    }

    public void SetVisibility(bool enable)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.enabled = enable;
        }
    }

    public void Activate()
    {
        if (hasAnimation)
            animator.SetTrigger(triggerName);

        if (hasAudio)
            AudioController.Play(audioId, transform);
    }
}