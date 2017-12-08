using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ScreamerTriggerable : ZoneTriggerable
{
    [SerializeField] private bool isSpawned = false;
    [SerializeField] private float lifetimeDuration = 0f;
    [SerializeField] private GameObject screamerPrefab = null;

    protected bool IsInZone;
    protected bool IsTriggered;

    private VisualTransition visualTransition;
    private List<ScreamerActionComponent> lifetimeActions;
    private Screamer screamer;

    [UsedImplicitly]
    private void Awake()
    {
        visualTransition = GetComponentInParent<VisualTransition>();
        lifetimeActions = new List<ScreamerActionComponent>(GetComponentsInParent<ScreamerActionComponent>());

        SetupScreamerPrefab();
    }

    private void SetupScreamerPrefab()
    {
        var screamerObject = Instantiate(screamerPrefab, transform);
        var pos = screamerObject.transform.position;
        pos.Set(pos.x, -transform.position.y, pos.z);

        screamer = screamerObject.GetComponent<Screamer>();
        screamer.SetVisibility(isSpawned);
    }

    public override void TriggerEnter()
    {
        IsInZone = true; 
        HandleEnter();
    }

    public override void TriggerExit()
    {
        IsInZone = false; 
        HandleExit();
    }

    public void TriggerSpawn()
    {
        if (!IsTriggered)
        {
            IsTriggered = true;
            StartCoroutine(SpawnCoroutine());
        }
    }

    public IEnumerator SpawnCoroutine()
    {
        if (!isSpawned)
        {
            var appearTransition = visualTransition.Appear(screamer);
            yield return new WaitForSeconds(appearTransition);
        }

        Action();
        yield return new WaitForSeconds(lifetimeDuration);

        var disappearTransition = visualTransition.Disappear(screamer); 
        yield return new WaitForSeconds(disappearTransition);

        enabled = false;
    }

    private void Action()
    {
        lifetimeActions.ForEach(a => a.TriggerAction(lifetimeDuration));
        screamer.Activate();
    }

    protected virtual void HandleEnter()
    {
        
    }

    protected virtual void HandleExit()
    {
        
    }
}
