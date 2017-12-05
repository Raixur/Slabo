using System.Collections.Generic;
using UnityEngine;

public class LockableDoor : MonoBehaviour
{
    [SerializeField]
    private List<Collider> doorColliders = new List<Collider>();
    [SerializeField]
    private Collider lockCollider;

    public bool IsLocked = true;

    public void OnValidate()
    {
        SetLock(IsLocked);
    }

    public void Awake()
    {
        SetLock(IsLocked);
    }

    public void SetLock(bool isLocked)
    {
        IsLocked = isLocked;
        doorColliders.ForEach(c => c.enabled = !isLocked);
        if (lockCollider != null)
            lockCollider.enabled = isLocked;
    }
}
