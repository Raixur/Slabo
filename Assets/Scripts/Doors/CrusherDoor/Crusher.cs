using JetBrains.Annotations;
using UnityEngine;

public class Crusher : MonoBehaviour
{
    [SerializeField] private LockableDoor door;
    [SerializeField] private LockBolts lockBolts;
    [SerializeField] private float requiredScore;

    private float currentScore = 0f;

    [UsedImplicitly]
    private void OnTriggerEnter(Collider other)
    {
        var bodyPart = other.GetComponent<CrusherBodyPart>();
        if (bodyPart != null)
        {
            currentScore += bodyPart.Score;
            bodyPart.DestroyBodyPart();
            TryActivate();
        }
    }

    private void TryActivate()
    {
        if (currentScore > requiredScore)
        {
            door.SetLock(false);
            lockBolts.ToggleLock();
            GetComponent<Collider>().enabled = false;
        }
    }
}