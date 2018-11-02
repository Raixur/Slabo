using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LockableDoor))]
public class LeverDoorSystem : MonoBehaviour
{
    private bool[] doorStates = {false, false, false, false};
    private LockableDoor lockableDoor;

    private void Awake()
    {
        lockableDoor = GetComponent<LockableDoor>();
    }

	void Start ()
	{
	    var doors = GetComponentsInChildren<LeverDoor>().ToList();
	    var levers = GetComponentsInChildren<Lever>().ToList();

	    if (doors.Count < CodeGenerator.RequiredLeverDoors)
	    {
	        Debug.LogError("Not enough doors");
	    }

	    if (levers.Count < CodeGenerator.RequiredLevers)
	    {
	        Debug.LogError("Not enough levers");
	    }

	    var rnd = new System.Random();
	    var doorCombinations = CodeGenerator.Instance.DoorCombinations;
	    for (var i = 0; i < CodeGenerator.RequiredLevers; i++)
	    {
	        var leverIndex = rnd.Next(levers.Count);
	        foreach (var combinationIndex in doorCombinations[i])
	        {
	            var doorIndex = combinationIndex;
	            levers[leverIndex].Toggled += (sender, args) =>
	            {
	                doors[doorIndex].ToggleLock();
                    UpdateDoor(doorIndex);
	            };
	        }

            levers.RemoveAt(leverIndex);
	    }
    }

    private void UpdateDoor(int index)
    {
        doorStates[index] = !doorStates[index];

        if(!lockableDoor.IsOpened && IsOpened()) 
            lockableDoor.SetOpen(true);

        if(lockableDoor.IsOpened && !IsOpened())
            lockableDoor.SetOpen(false);
    }

    private bool IsOpened()
    {
        return doorStates.Aggregate(true, (current, s) => current && s);
    }
}
