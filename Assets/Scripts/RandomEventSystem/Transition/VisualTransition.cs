using UnityEngine;

public abstract class VisualTransition : MonoBehaviour
{
    public abstract float Appear(Screamer screamer);
    public abstract float Disappear(Screamer screamer);
}
