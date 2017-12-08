using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

public class ZoneSwitchMultiple : MonoBehaviour
{
    [SerializeField] private int count = 1;
    private List<TriggerZone> zones;

    [UsedImplicitly]
    private void Awake()
    {
        zones = new List<TriggerZone>(GetComponentsInChildren<TriggerZone>());
        zones.ForEach(z => z.gameObject.SetActive(false));
    }

    [UsedImplicitly]
    private void Start()
    {
        var rnd = new Random();
        for (var i = 0; i < count; i++)
        {
            var index = rnd.Next(zones.Count);
            zones[index].gameObject.SetActive(true);
            zones.RemoveAt(index);
        }
    }
}