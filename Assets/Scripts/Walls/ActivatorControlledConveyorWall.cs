using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorControlledConveyorWall : ActivatablePuzzlePiece
{
    [SerializeField] ConveyerWall wallPart;
    [SerializeField] private float playerMoveForcePerActivator = 25f;
    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        wallPart.PlayerMoveForceAmount = 0.0f;
    }
    public override void Activate()
    {
        //increment index then multiply by force per activator then set the wall's force to that value
        wallPart.PlayerMoveForceAmount = playerMoveForcePerActivator * ++index;
    }

    public override void Deactivate(GameObject caller)
    {
        if (index == 0) return;
        wallPart.PlayerMoveForceAmount = playerMoveForcePerActivator * --index;
    }

}