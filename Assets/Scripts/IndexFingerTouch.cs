using UnityEngine;
using VRTK;

[RequireComponent(typeof(VRTK_ControllerEvents))]

public class IndexFingerTouch : VRTK_InteractTouch
{
    private VRTK_ControllerEvents controller;
    private Collider indexFingerCollider;

    private bool isFingerPointing = false;

    [SerializeField]
    private GameObject indexFingerColliderContainer = null;

    public void Start()
    {
        controller = GetComponent<VRTK_ControllerEvents>();
        indexFingerCollider = indexFingerColliderContainer.GetComponent<Collider>();
    }

    protected override void FixedUpdate()
    {
        if (!isFingerPointing && controller.gripTouched && !controller.triggerTouched)
        {
            SetFingerPointing();
        }

        if (isFingerPointing && (!controller.gripTouched || controller.triggerTouched))
        {
            ResetFingerPointing();
        }

        base.FixedUpdate();
    }

    public void SetFingerPointing()
    {
        foreach (var controllerCollider in ControllerColliders())
        {
            controllerCollider.enabled = false;
        }
        indexFingerCollider.enabled = true;
    }

    public void ResetFingerPointing()
    {
        foreach (var controllerCollider in ControllerColliders())
        {
            controllerCollider.enabled = true;
        }
        indexFingerCollider.enabled = false;
    }

    protected override void CreateTouchCollider()
    {
        base.CreateTouchCollider();

        if (indexFingerColliderContainer != null)
        {
            indexFingerColliderContainer.transform.SetParent(controllerCollisionDetector.transform);
            indexFingerColliderContainer.transform.localScale = controllerCollisionDetector.transform.localScale;
        }
    }
}
