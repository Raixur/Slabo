using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LockBolts : MonoBehaviour
{
    [SerializeField] private bool isOpened;
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

    public void SetOpen(bool isOpening)
    {
        if (isOpened != isOpening)
        {
            isOpened = isOpening;
            animator.SetBool("Open", isOpened);
        }
        
    }
}
