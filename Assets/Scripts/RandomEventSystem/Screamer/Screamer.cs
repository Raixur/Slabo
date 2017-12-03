using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screamer : MonoBehaviour, IZoneTriggerable
{
    [SerializeField] private VisualTransition visualTransition;

    [SerializeField] private float lifetimeDuration = 0f;
    [SerializeField] private List<ScreamerActionComponent> lifetimeActions;

    protected bool IsInZone;
    protected bool IsTriggered;

    public void TriggerEnter()
    {
        IsInZone = true; 
        HandleEnter();
    }

    public void TriggerExit()
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
        var appearTransition = visualTransition.Appear();
        yield return new WaitForSeconds(appearTransition);

        Action();
        yield return new WaitForSeconds(lifetimeDuration);

        var disappearTransition = visualTransition.Disappear(); 
        yield return new WaitForSeconds(disappearTransition);

        enabled = false;
    }

    private void Action()
    {
        lifetimeActions.ForEach(a => a.TriggerAction(lifetimeDuration));
        HandleAction();
    }

    protected virtual void HandleEnter()
    {
        
    }

    protected virtual void HandleExit()
    {
        
    }

    protected virtual void HandleAction()
    {

    }
}
