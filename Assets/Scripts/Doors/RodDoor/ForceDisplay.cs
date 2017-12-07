using UnityEngine;

public class ForceDisplay : MonoBehaviour
{
    [SerializeField] private Vector3 rotationVector = Vector3.forward;

    [SerializeField] private float startAngle = 0f;
    [SerializeField] private float endAngle = 270f;

    public void SetNormalizedRotation(float normalAngle)
    {
        SetRotation((endAngle - startAngle) / 100 * normalAngle);
    }

    public void SetRotation(float angle)
    {
        transform.rotation = transform.parent.rotation;
        transform.Rotate(rotationVector, angle);
    }
}
