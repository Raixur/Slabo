using System;
using UnityEngine;
using VRTK;

[RequireComponent(typeof(VRTK_InteractableObject))]
[RequireComponent(typeof(VRTK_InteractControllerAppearance))]
public class CrusherBodyPart : MonoBehaviour
{
    public float Score;

    private VRTK_InteractableObject interactableObject;

    public void Awake()
    {
        interactableObject = GetComponent<VRTK_InteractableObject>();
    }

    public void DestroyBodyPart()
    {
        interactableObject.ForceStopInteracting();
        OnDestroyed();
        Destroy(gameObject);
    }

    public event EventHandler Destroyed;

    protected virtual void OnDestroyed()
    {
        if (Destroyed != null)
            Destroyed(this, EventArgs.Empty);
    }
}
