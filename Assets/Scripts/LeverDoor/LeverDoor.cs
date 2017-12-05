using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LeverDoor : MonoBehaviour
{
    [SerializeField] private bool isOpened = false;
    private Animator animator;
    
    [UsedImplicitly]
    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("Open", isOpened);
    }

    public void ToggleLock()
    {
        isOpened = !isOpened;
        animator.SetBool("Open", isOpened);
    }
}
