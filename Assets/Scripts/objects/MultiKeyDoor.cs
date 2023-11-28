using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiKeyDoor : DoorBehaviour
{
    [SerializeField] List<bool> active;
    int index = 0;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void Activate()
    {
        active[index] = true;
        index++;

        if (index >= active.Count)
        {
            OpenDoor();
        }
    }

    public override void Deactivate(GameObject caller)
    {
        active[index] = false;
        index--;

        CloseDoor();
    }
}
