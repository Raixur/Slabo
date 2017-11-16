using System.Collections.Generic;
using UnityEngine;

public class LockableDoor : MonoBehaviour
{
    [SerializeField]
    private List<Collider> doorColliders = new List<Collider>();
    [SerializeField]
    private Collider lockCollider;

    public bool DefaultLock = true;

    public void OnValidate()
    {
        SetLock(DefaultLock);
    }

    public void Awake()
    {
        SetLock(DefaultLock);
    }

    public void SetLock(bool isLocked)
    {
        doorColliders.ForEach(c => c.enabled = !isLocked);

        if (lockCollider != null)
            lockCollider.enabled = isLocked;
    }
}
