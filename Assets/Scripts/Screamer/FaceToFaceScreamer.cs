using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class FaceToFaceScreamer : MonoBehaviour
{
    [SerializeField] private GameObject screamer = null;
    [SerializeField] private float facingAngle = 20f;
    [SerializeField] private float height = 1f;

    private Transform cameraTransform;
    private Vector3 screamerPosition;

    [UsedImplicitly]
    private void Start()
    {
        StartCoroutine(InitCameraCoroutine());

        screamerPosition = screamer.transform.position;
        screamerPosition.y += height;
    }

    private IEnumerator InitCameraCoroutine()
    {
        do
        {
            cameraTransform = VRTK_SDK_Bridge.GetHeadsetCamera();
            yield return null;
        } while (cameraTransform == null);
    }

    [UsedImplicitly]
    private void OnTriggerStay(Collider other)
    {

        var isFacing = Vector3.Angle(cameraTransform.forward, screamerPosition - cameraTransform.position) < facingAngle;
        if (!screamer.activeSelf && isFacing)
        {
            screamer.SetActive(true);
        }
        if (screamer.activeSelf && !isFacing)
        {
            screamer.SetActive(false);
        }
    }
}
