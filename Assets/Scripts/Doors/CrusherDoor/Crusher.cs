using AudioSDK;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class Crusher : MonoBehaviour
{
    [SerializeField] private LockableDoor door;
    [SerializeField] private LockBolts lockBolts;
    [SerializeField] private float requiredScore;

    [SerializeField] private string engineAudio = "";
    [SerializeField] private string bodyPartCrushingAudio = "";

    private float currentScore = 0f;

    private void Awake()
    {
        VRTK_SDKManager.instance.LoadedSetupChanged += (sender, args) => AudioController.Play(engineAudio, transform);
    }

    [UsedImplicitly]
    private void OnTriggerEnter(Collider other)
    {
        var bodyPart = other.GetComponent<CrusherBodyPart>();
        if (bodyPart != null)
        {
            currentScore += bodyPart.Score;
            bodyPart.DestroyBodyPart();
            PlayBodyPartCrushing();
            TryActivate();
        }
    }

    private void TryActivate()
    {
        if (currentScore > requiredScore)
        {
            door.SetOpen(true);
            lockBolts.ToggleLock();
            GetComponent<Collider>().enabled = false;
        }
    }

    private void PlayBodyPartCrushing()
    {
        AudioController.Play(bodyPartCrushingAudio, transform);
    }
}