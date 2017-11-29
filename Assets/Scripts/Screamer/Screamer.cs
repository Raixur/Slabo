using System;
using JetBrains.Annotations;
using UnityEngine;

public class Screamer : MonoBehaviour
{
    public float SpawnDuration = 0f;
    public float ActionDuration = 0f;
    public float DespawnDuration = 0f;

    [SerializeField] private ScreamerSpawner spawner;
    public bool IsBlocking = true;

    protected bool IsInZone;

    public event EventHandler Trigger;

    [UsedImplicitly]
    protected virtual void Start()
    {
        spawner.Entered += SpawnerOnEntered;
        spawner.Left += SpawnerOnLeft;
    }

    private void SpawnerOnLeft(object sender, ScreamerEventArgs args)
    {
        IsInZone = false;
    }

    private void SpawnerOnEntered(object o, ScreamerEventArgs screamerEventArgs)
    {
        IsInZone = true;
    }

    public virtual void Spawn()
    {
        
    }

    public virtual void Action()
    {
        
    }

    public virtual void Despawn()
    {
        
    }

    protected virtual void OnTrigger()
    {
        if (Trigger != null)
            Trigger(this, EventArgs.Empty);
    }
}
