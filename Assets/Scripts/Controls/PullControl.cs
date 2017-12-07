using UnityEngine;
using VRTK;

public class PullControl : VRTK_Control
{
    private Vector3 restPosition;

    [SerializeField] private float minDistance = 0f;
    [SerializeField] private float maxDistance = 2f; 
    [SerializeField] private float activationDistance = 1f;

    [SerializeField] private VRTK_InteractableObject handle = null;

    protected override void InitRequiredComponents()
    {
        restPosition = transform.position;
        if (handle == null)
        {
            handle = GetComponent<VRTK_InteractableObject>() ?? gameObject.AddComponent<VRTK_InteractableObject>();
        }
    }

    protected override bool DetectSetup()
    {
        return true;
    }

    protected override ControlValueRange RegisterValueRange()
    {
        return new ControlValueRange
        {
            controlMin = minDistance,
            controlMax = maxDistance
        };
    }

    protected override void HandleUpdate()
    {
        value = GetDistance();
        if (value >= activationDistance)
        {
            OnActivationPulled(SetControlEvent());
        }
        if (value >= maxDistance)
        {
            if (handle.IsGrabbed())
            {
                handle.ForceStopInteracting();
            }
            
            OnReleased(SetControlEvent());
        }
    }

    private float GetDistance()
    {
        return Vector3.Distance(handle.gameObject.transform.position, restPosition);
    }

    /// <summary>
    /// Emitted when the handle has reached its activation distance.
    /// </summary>
    public event Button3DEventHandler ActivationPulled;

    /// <summary>
    /// Emitted when the handle reached max distance and released.
    /// </summary>
    public event Button3DEventHandler Released;

    private void OnActivationPulled(Control3DEventArgs e)
    {
        if (ActivationPulled != null)
        {
            ActivationPulled(this, e);
        }
    }

    private void OnReleased(Control3DEventArgs e)
    {
        if (Released != null)
        {
            Released(this, e);
        }
    }
}