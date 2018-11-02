using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class FacingScreamerTriggerable : ScreamerTriggerable
{
    [SerializeField] private float facingAngle = 40f;
    private Transform cameraTransform;

    [UsedImplicitly]
    private void Start()
    {
        VRTK_SDKManager.instance.LoadedSetupChanged +=
            (sender, args) => cameraTransform = VRTK_SDK_Bridge.GetHeadsetCamera();   
    }

    [UsedImplicitly]
    private void Update()
    {
        if (IsInZone && IsFaced(cameraTransform))
            TriggerSpawn();
    }

    private bool IsFaced(Transform facingTransform)
    {
        return Vector3.Angle(facingTransform.forward, transform.position - facingTransform.position) < facingAngle;
    }
}
