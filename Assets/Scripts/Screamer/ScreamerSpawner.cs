using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class ScreamerEventArgs
{
    public Transform Camera { get; set; }
    public Screamer Screamer { get; set; }
}

public delegate void ScreamerEventHandler(object sender, ScreamerEventArgs args);

public class ScreamerQueue
{
    private readonly Queue<Screamer> screamers;

    public ScreamerQueue(IEnumerable<Screamer> initializeList)
    {
        Screamer = null;
        screamers = new Queue<Screamer>(initializeList);
        if (screamers.Count == 0)
            Debug.LogError("Screamer list is empty!");

        SubscribeDespawnChain();
    }

    public Screamer Screamer { get; private set; }

    private void ActivateNext(object sender, EventArgs args)
    {
        Screamer.DespawnEnd -= ActivateNext;
        SubscribeDespawnChain();
    }

    private void SubscribeDespawnChain()
    {
        Screamer = screamers.Dequeue();
        if (Screamer != null)
            Screamer.DespawnEnd += ActivateNext;
    }
}


public class ScreamerSpawner : MonoBehaviour
{
    [SerializeField] private List<Screamer> screamers;

    private ScreamerQueue screamerQueue;
    private Transform cameraTransform;
    private bool triggered;

    public event ScreamerEventHandler Entered;
    public event ScreamerEventHandler Left;
    public event ScreamerEventHandler Faced;
    public event ScreamerEventHandler Unfaced;

    [UsedImplicitly]
    private void Start()
    {
        VRTK_SDKManager.instance.LoadedSetupChanged +=
            (sender, args) => cameraTransform = VRTK_SDK_Bridge.GetHeadsetCamera();

        screamerQueue = new ScreamerQueue(screamers);
    }

    [UsedImplicitly]
    private void OnTriggerEnter(Collider other)
    {
        if (VRTK_PlayerObject.IsPlayerObject(other.gameObject))
            OnEntered();
    }

    [UsedImplicitly]
    private void OnTriggerExit(Collider other)
    {
        if (VRTK_PlayerObject.IsPlayerObject(other.gameObject))
            OnLeft();
    }

    [UsedImplicitly]
    private void OnTriggerStay(Collider other)
    {
        if (VRTK_PlayerObject.IsPlayerObject(other.gameObject))
        {
            var isFacing = screamerQueue.Screamer.IsFaced(cameraTransform);
            if (!triggered && isFacing)
            {
                triggered = true;
                OnFaced();
            }
            if (triggered && !isFacing)
            {
                triggered = false;
                OnUnfaced();
            }
        }
    }

    protected virtual ScreamerEventArgs GetEventPayload()
    {
        return new ScreamerEventArgs
        {
            Camera = cameraTransform,
            Screamer = screamerQueue.Screamer
        };
    }

    protected virtual void OnEntered()
    {
        if (Entered != null)
            Entered(this, GetEventPayload());
    }

    protected virtual void OnLeft()
    {
        if (Left != null)
            Left(this, GetEventPayload());
    }

    protected virtual void OnFaced()
    {
        if (Faced != null)
            Faced(this, GetEventPayload());
    }

    protected virtual void OnUnfaced()
    {
        if (Unfaced != null)
            Unfaced(this, GetEventPayload());
    }
}
