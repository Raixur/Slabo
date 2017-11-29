using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class ScreamerEventArgs
{
    public Screamer Screamer { get; set; }
}

public delegate void ScreamerEventHandler(object sender, ScreamerEventArgs args);


public class ScreamerSpawner : MonoBehaviour
{
    [SerializeField] private List<Screamer> screamers;

    private Queue<Screamer> screamerQueue;
    private Screamer activeScreamer;

    public event ScreamerEventHandler Entered;
    public event ScreamerEventHandler Left;

    public event ScreamerEventHandler SpawnStart;
    public event ScreamerEventHandler SpawnEnd;
    public event ScreamerEventHandler DespawnStart;
    public event ScreamerEventHandler DespawnEnd;

    [UsedImplicitly]
    private void Start()
    {
        screamerQueue = new Queue<Screamer>(screamers);
        activeScreamer = screamerQueue.Dequeue();
        activeScreamer.Trigger += ActivateScreamer;
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


    public void ActivateScreamer(object screamer, EventArgs screamerEventArgs)
    {
        StartCoroutine(ActivateScreamerCoroutine((Screamer)screamer));
    }

    private IEnumerator ActivateScreamerCoroutine(Screamer screamer)
    {
        if (!screamer.IsBlocking)
            SubscribeNext();

        OnSpawnStart(screamer);
        screamer.Spawn();
        yield return new WaitForSeconds(screamer.SpawnDuration);
        OnSpawnEnd(screamer);

        screamer.Action();
        yield return new WaitForSeconds(screamer.ActionDuration);

        OnDespawnStart(screamer);
        screamer.Despawn();
        yield return new WaitForSeconds(screamer.DespawnDuration);
        OnDespawnEnd(screamer);

        if (screamer.IsBlocking)
            SubscribeNext();
    }

    private void SubscribeNext()
    {
        activeScreamer.Trigger -= ActivateScreamer;
        if (screamerQueue.Count != 0)
        {
            activeScreamer = screamerQueue.Dequeue();
            activeScreamer.Trigger += ActivateScreamer;
        }
    }

    protected virtual ScreamerEventArgs GetEventPayload(Screamer screamer)
    {
        return new ScreamerEventArgs
        {
            Screamer = screamer
        };
    }

    protected virtual void OnEntered()
    {
        Debug.Log("Entered");
        if (Entered != null)
            Entered(this, GetEventPayload(activeScreamer));
    }

    protected virtual void OnLeft()
    {
        Debug.Log("Left");
        if (Left != null)
            Left(this, GetEventPayload(activeScreamer));
    }

    protected virtual void OnSpawnStart(Screamer screamer)
    {
        if (SpawnStart != null)
            SpawnStart(this, GetEventPayload(screamer));
    }

    protected virtual void OnSpawnEnd(Screamer screamer)
    {
        if (SpawnEnd != null)
            SpawnEnd(this, GetEventPayload(screamer));
    }

    protected virtual void OnDespawnStart(Screamer screamer)
    {
        if (DespawnStart != null)
            DespawnStart(this, GetEventPayload(screamer));
    }

    protected virtual void OnDespawnEnd(Screamer screamer)
    {
        if (DespawnEnd != null)
            DespawnEnd(this, GetEventPayload(screamer));
    }
}
