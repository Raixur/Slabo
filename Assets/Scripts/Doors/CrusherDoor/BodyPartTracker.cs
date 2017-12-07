using JetBrains.Annotations;
using UnityEngine;

public class BodyPartTracker : MonoBehaviour
{
    [UsedImplicitly]
    private void OnTriggerExit(Collider other)
    {
        var bodyPartComponent = other.GetComponent<CrusherBodyPart>();
        if (bodyPartComponent != null)
        {
            bodyPartComponent.DestroyBodyPart();
        }
    }
}