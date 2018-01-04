using System.Collections.Generic;
using UnityEngine;

public class LockableDoor : MonoBehaviour
{
    [SerializeField]
    private List<Collider> doorColliders = new List<Collider>();
    [SerializeField]
    private Collider lockCollider;

    public bool IsOpened;

    public void OnValidate()
    {
        SetOpen(IsOpened);
    }

    public void Awake()
    {
        SetOpen(IsOpened);
    }

    public void SetOpen(bool isOpened)
    {
        if (IsOpened != isOpened)
        {
            IsOpened = isOpened;
            doorColliders.ForEach(c => c.enabled = isOpened);
            if (lockCollider != null)
                lockCollider.enabled = !isOpened;
        }
    }
}
