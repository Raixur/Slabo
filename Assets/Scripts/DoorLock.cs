using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DoorLock : MonoBehaviour
{
    private bool isLocked;
    private FixedJoint lockJoint;

    public Rigidbody Door;

    public bool IsLocked
    {
        get { return isLocked; }
        set
        {
            if (isLocked != value)
            {
                isLocked = value;
                if (isLocked)
                    LockDoor();
                else
                    UnlockDoor();
            }
        }
    }

    public void Start()
    {
        if (IsLocked)
        {
            
        }
    }

    public void LockDoor()
    {
        lockJoint = gameObject.AddComponent<FixedJoint>();
        lockJoint.connectedBody = Door;
    }

    private void UnlockDoor()
    {
        Destroy(lockJoint);
    }
}
