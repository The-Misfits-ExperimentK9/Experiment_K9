using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvexWall : MonoBehaviour
{
    [SerializeField]
    WallBehaviour wall1;

    [SerializeField]
    WallBehaviour wall2;

    PlayerBehaviour player;

    MovementController_2D controller2D;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("NewPlayer").GetComponent<PlayerBehaviour>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("triggered");
        if (other.gameObject.layer == LayerInfo.PLAYER && !player.is3D)
        {
            if (controller2D.GetCurrentWall() == wall1)
                controller2D.HandleWallCollision(wall2.GetComponent<Collider>(), wall2, true);
            else if (controller2D.GetCurrentWall() == wall2)
                controller2D.HandleWallCollision(wall1.GetComponent<Collider>(), wall1, true);
        }
    }
}
