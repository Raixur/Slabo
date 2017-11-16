using System;
using System.Collections;
using UnityEngine;

public class Screamer : MonoBehaviour
{
    [SerializeField] private bool SpawnEnabled = true;
    [SerializeField] private bool DespawnEnabled = true;
    [SerializeField] private GameObject character;
    
    [SerializeField] private bool isAnimated = false;
    [SerializeField] private RuntimeAnimatorController animatorController = null;
    [SerializeField] private string trigger = "OnSpawn";
    private Animator animator;

    [SerializeField] private float spawnDuration = 0.1f;
    [SerializeField] private float lifetimeDuration = 0f;
    [SerializeField] private float despawnDuration = 0.1f;

    public event EventHandler<SpawnEventArgs> Spawn;
    public event EventHandler<SpawnEventArgs> Despawn;

    public void Start()
    {
        character = character ?? gameObject;

        if (isAnimated)
        {
            animator = character.GetComponent<Animator>() ?? character.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController ?? new RuntimeAnimatorController();
        }
    }

    public void InitSpawn()
    {
        StartCoroutine(LifetimeCoroutine());
    }

    public IEnumerator LifetimeCoroutine()
    {
        OnSpawn();
        yield return new WaitForSeconds(spawnDuration);

        character.SetActive(true);
        if (isAnimated)
            animator.SetTrigger(trigger);
        yield return new WaitForSeconds(lifetimeDuration);

        OnDespawn();
        character.SetActive(false);
    }

    protected virtual void OnSpawn()
    {
        if (SpawnEnabled && Spawn != null)
            Spawn(this, new SpawnEventArgs { Duration = spawnDuration + 0.01f });
    }

    protected virtual void OnDespawn()
    {
        if (DespawnEnabled && Despawn != null)
            Despawn(this, new SpawnEventArgs {Duration = despawnDuration});
    }
}

public class SpawnEventArgs : EventArgs
{
    public float Duration { get; set; }
}