using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatableWall : ActivatablePuzzlePiece
{
    [SerializeField] WallBehaviour wallPart;
    [SerializeField] private List<GameObject> activator;
    [SerializeField] List<bool> active;
    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Activate()
    {
        active[index] = true;
        index++;

        wallPart.Active = true;
    }

    public override void Deactivate(GameObject caller)
    {
        active[index] = false;
        index--;

        wallPart.Active = false;
    }
}
