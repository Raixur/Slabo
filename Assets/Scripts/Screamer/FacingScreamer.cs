using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class FacingScreamer : Screamer
{
    [SerializeField] private float facingAngle = 40f;
    [SerializeField] private GameObject model = null;

    [SerializeField] private Animator animator = null;
    [SerializeField] private string triggerName = "";

    private bool isTriggered;
    private Transform cameraTransform;

    [UsedImplicitly]
    protected override void Start()
    {
        base.Start();
        VRTK_SDKManager.instance.LoadedSetupChanged +=
            (sender, args) => cameraTransform = VRTK_SDK_Bridge.GetHeadsetCamera();
    }

    [UsedImplicitly]
    private void Update()
    {
        if (IsInZone && !isTriggered && IsFaced(cameraTransform))
        {
            isTriggered = true;
            OnTrigger();            
        }
    }

    public override void Action()
    {
        model.SetActive(true);
        if(animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }

    public override void Despawn()
    {
        model.SetActive(false);
    }

    public bool IsFaced(Transform facingTransform)
    {
        var facing = Vector3.Angle(facingTransform.forward, transform.position - facingTransform.position) <
                     facingAngle;
        
        return facing;
    }
}
