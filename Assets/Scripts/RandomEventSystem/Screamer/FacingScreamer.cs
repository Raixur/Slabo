using AudioSDK;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class FacingScreamer : Screamer
{
    [SerializeField] private float facingAngle = 40f;
    [SerializeField] private GameObject model = null;

    [SerializeField] private Animator animator = null;
    [SerializeField] private string triggerName = "";

    [SerializeField] private string audioId = "";

    private Transform cameraTransform;
    private bool hasAnimation;
    private bool hasAudio;

    [UsedImplicitly]
    private void Start()
    {
        VRTK_SDKManager.instance.LoadedSetupChanged +=
            (sender, args) => cameraTransform = VRTK_SDK_Bridge.GetHeadsetCamera();

        hasAnimation = animator != null && !string.IsNullOrEmpty(triggerName);
        hasAudio = !string.IsNullOrEmpty(audioId);
    }

    [UsedImplicitly]
    private void Update()
    {
        if (IsInZone && IsFaced(cameraTransform))
            TriggerSpawn();
    }

    protected override void HandleAction()
    {
        if(hasAnimation)
            animator.SetTrigger(triggerName);

        if(hasAudio)
            AudioController.Play(audioId);
    }

    private bool IsFaced(Transform facingTransform)
    {
        return Vector3.Angle(facingTransform.forward, transform.position - facingTransform.position) < facingAngle;
    }
}
