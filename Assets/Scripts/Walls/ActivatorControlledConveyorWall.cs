using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorControlledConveyorWall : ActivatablePuzzlePiece
{
    [SerializeField] ConveyerWall wallPart;
    [SerializeField] private List<GameObject> activator;
    [SerializeField] List<bool> active;
    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        wallPart.PlayerMoveForceAmount = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        wallPart.PlayerMoveForceAmount = 25.0f * CheckActivators();
    }

    public override void Activate()
    {
        active[index] = true;
        index++;

        if (index >= active.Count)
        {
            index = 0;
        }
    }

    public override void Deactivate()
    {
        active[index] = false;
        index--;

        if (index >= active.Count)
        {
            index = 0;
        }
    }

    private int CheckActivators()
    {
        int activatorsActive = 0;

        for (int i = 0; i < active.Count; i++)
        {
            if (active[i] == true)
            {
                activatorsActive++;
            }
        }

        return activatorsActive;
    }
}