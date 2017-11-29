using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(ScreamerSpawner))]
public class ScreamerFlickering : MonoBehaviour
{
    [SerializeField] private List<LightFlickering> affectedLight = null;
    [SerializeField] private bool flickerOnSpawn = true;
    [SerializeField] private bool flickerOnDespawn = true;

    [UsedImplicitly]
    private void Start()
    {
        var spawner = GetComponent<ScreamerSpawner>();

        if (flickerOnSpawn)
            spawner.SpawnStart += (sender, args) =>
                affectedLight.ForEach(l => l.FlickerByTime(args.Screamer.SpawnDuration));

        if (flickerOnDespawn)
            spawner.DespawnStart += (sender, args) =>
                affectedLight.ForEach(l => l.FlickerByTime(args.Screamer.DespawnDuration));
    }
}
