using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockBolts : MonoBehaviour
{
    private bool isLocked = true;
    private bool isUnlocked = false;

    [SerializeField] private Vector3 unlockPosition;
    [SerializeField] private float unlockSpeed;

    public void Update()
    {
        if(!isUnlocked && !isLocked)
        {
            var step = unlockSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, unlockPosition, step);

            if (transform.position == unlockPosition)
                isUnlocked = true;
        }
    }

    public void StartUnlocking()
    {
        isLocked = false;
    }
}
