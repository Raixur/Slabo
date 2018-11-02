using System;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

public class BodyPartSpawner : MonoBehaviour
{
    private readonly Random rnd = new Random();

    [SerializeField] private GameObject bodyPartPrefab;
    [SerializeField] private Transform spawnTransform;

    [SerializeField] private float minPoints = 5f;
    [SerializeField] private float maxPoints = 10f;

    private CrusherBodyPart previousBodyPart;

    private float NextScore { get { return minPoints + (float) rnd.NextDouble() * (maxPoints - minPoints); } }

    [UsedImplicitly]
    private void Start()
    {
        CreateBodyPart();
    }

    private void CreateBodyPart(object sender, EventArgs e)
    {
        previousBodyPart.Destroyed -= CreateBodyPart;
        CreateBodyPart();
    }

    private void CreateBodyPart()
    {
        var bodyPart = Instantiate(bodyPartPrefab, spawnTransform.position, spawnTransform.rotation);
        previousBodyPart = bodyPart.AddComponent<CrusherBodyPart>();
        previousBodyPart.Score = NextScore;
        previousBodyPart.Destroyed += CreateBodyPart;
    }
}
