using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorConveyerWall : ConveyerWall
{
    // The activators for this variety of conveyer wall.
    [SerializeField] private List<GameObject> activator;
    [SerializeField] List<bool> active;
    int index = 0;
    int activeCount = 0;

    // Start is called before the first frame update
    protected override void Start()
    {
        // Initially, the conveyer wall doesn't move the player at all.
        PlayerMoveForceAmount = 0.0f;
        base.Start();
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
    }



    void CheckActivators()
    {
        active[index] = true;
        index++;

        for (int i = 0; i < active.Count; i++)
        {
            if (active[i] == true)
            {
                activeCount++;
            }
        }

        PlayerMoveForceAmount = 25.0f * activeCount;
    }
}
