using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class FacingScreamer : Screamer
{
    [SerializeField] private float facingAngle = 40f;
    [SerializeField] private GameObject model = null;

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
            Debug.Log("Facing screamer triggered");
            isTriggered = true;
            OnTrigger();            
        }
    }

    public override void Action()
    {
        model.SetActive(true);
    }

    public override void Despawn()
    {
        model.SetActive(false);
    }

    public bool IsFaced(Transform facingTransform)
    {
        var facing = Vector3.Angle(facingTransform.forward, transform.position - facingTransform.position) <
                     facingAngle;

        Debug.Log(facing);
        return facing;
    }
}
