using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterCorner : MonoBehaviour {
    [SerializeField] private WallBehaviour leftWall;
    [SerializeField] private WallBehaviour rightWall;
    [SerializeField] private Vector3 rightOffset = new(6, 0, 0);
    [SerializeField] private Vector3 leftOffset = new(-6, 0, 0);
    [SerializeField] private float xOffsetAmount = 6f;
    private Vector3 gizmoDrawLocation = new();
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerInfo.PLAYER) {

            BeginWallTransfer(other);
        }
    }
    void BeginWallTransfer(Collider playerCollider) {
        if (PlayerBehaviour.Instance.IsIn3D()) return;
        if (!playerCollider.TryGetComponent(out MovementController_2D player)) {
            return;
        }
        
        var playerCurrentWall = player.GetCurrentWall();
        if (playerCurrentWall == rightWall) {
            TransitionRight(player);

        }
        else if (playerCurrentWall == leftWall) {

            TransitionLeft(player);
        }

    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(gizmoDrawLocation, .5f);
        
    }
    void TransitionRight(MovementController_2D player) {
        Debug.Log("move right");
        player.CanMove = false;
        player.transform.position = transform.position;
        player.transform.forward = player.transform.right;
        player.transform.position += player.transform.right * xOffsetAmount;
        gizmoDrawLocation = player.transform.position;
        player.SetCurrentWall(rightWall);
        player.LockPlayerMovementInForwardDirection();
        player.CanMove = true;

    }
    void TransitionLeft(MovementController_2D player) {
        Debug.Log("move left");
        player.CanMove = false;
        player.transform.position = transform.position;
        player.transform.forward = -player.transform.right;
        player.transform.position -= player.transform.right * xOffsetAmount;
        gizmoDrawLocation = player.transform.position;
        player.SetCurrentWall(leftWall);
        player.LockPlayerMovementInForwardDirection();
        player.CanMove = true;

    }


}
