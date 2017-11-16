using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PullControl))]
public class RodDoorChainControl : MonoBehaviour
{
    private PullControl pullControl;

    [SerializeField] private ForceDisplay forceDisplay = null;
    [SerializeField] private LockBolts lockBolts = null;
    [SerializeField] private LockableDoor door = null;

    public void Awake()
    {
        pullControl = GetComponent<PullControl>();
    }

    public void Start()
    {
        if(forceDisplay != null)
            pullControl.ValueChanged += (sender, args) => forceDisplay.SetNormalizedRotation(args.normalizedValue);

        if(lockBolts != null)
            pullControl.ActivationPulled += (sender, args) => lockBolts.gameObject.SetActive(false);

        if(door != null)
            pullControl.ActivationPulled += (sender, args) => door.SetLock(false);
    }

}
