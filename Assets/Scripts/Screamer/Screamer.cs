using System;
using System.Collections;
using UnityEngine;

public class Screamer : MonoBehaviour
{
    [SerializeField] private float spawnDuration = 0f;
    [SerializeField] private float actionDuration = 0f;
    [SerializeField] private float despawnDuration = 0f;
    [SerializeField] private float facingAngle = 40f;

    public event EventHandler SpawnStart;
    public event EventHandler SpawnEnd;

    public event EventHandler DespawnStart;
    public event EventHandler DespawnEnd;

    public bool IsFaced(Transform facingTransform)
    {
        return Vector3.Angle(facingTransform.forward, transform.position - facingTransform.position) < facingAngle;
    }

    public void InitAction()
    {
        StartCoroutine(InitActionCoroutine());
    }

    private IEnumerator InitActionCoroutine()
    {
        OnSpawnStart();
        Spawn();
        yield return new WaitForSeconds(spawnDuration);
        OnSpawnEnd();

        Action();
        yield return new WaitForSeconds(actionDuration);

        OnDespawnStart();
        Despawn();
        yield return new WaitForSeconds(despawnDuration);
        OnDespawnEnd();
    }

    protected virtual void Action()
    {

    }

    protected virtual void Spawn()
    {
        
    }

    protected virtual void Despawn()
    {
        
    }

    protected virtual void OnSpawnStart()
    {
        if (SpawnStart != null)
            SpawnStart(this, EventArgs.Empty);
    }

    protected virtual void OnSpawnEnd()
    {
        if (SpawnEnd != null)
            SpawnEnd(this, EventArgs.Empty);
    }

    protected virtual void OnDespawnStart()
    {
        if (DespawnStart != null)
            DespawnStart(this, EventArgs.Empty);
    }

    protected virtual void OnDespawnEnd()
    {
        if (DespawnEnd != null)
            DespawnEnd(this, EventArgs.Empty);
    }
}
