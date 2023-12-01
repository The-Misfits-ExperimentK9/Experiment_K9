using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerWall : WallBehaviour {
    [SerializeField] protected Rigidbody playerRb;
    [SerializeField] protected MovementController_2D player2D;
    public float PlayerMoveForceAmount;
    [SerializeField] protected Vector3 playerMoveDirection = Vector3.zero;

    protected virtual void Start() {
        if (playerMoveDirection == Vector3.zero) {
            playerMoveDirection = transform.forward;
        }
        else if (Mathf.Approximately(Mathf.Abs(playerMoveDirection.magnitude), 1)) {
            playerMoveDirection = playerMoveDirection.normalized;
        }
    }

    protected virtual void FixedUpdate() {
        MovePlayer();
    }

    protected virtual void MovePlayer() {
        if (playerRb != null) {
            if (PlayerBehaviour.Instance.IsIn3D()) {
                playerRb = null;
                return;
            }
            playerRb.AddForce(playerMoveDirection * PlayerMoveForceAmount);
            Debug.Log("Moving");
        }
    }
    private void OnCollisionEnter(Collision collision) {

        if (collision.gameObject.layer == LayerInfo.PLAYER) {
            if (collision.gameObject.TryGetComponent(out MovementController_2D player2D)) {
                
                playerRb = collision.gameObject.GetComponent<Rigidbody>();
                this.player2D = player2D;

            }

        }
    }
    private void OnCollisionExit(Collision collision) {
        Debug.Log("on collision exit");
        if (collision.gameObject.layer == LayerInfo.PLAYER) {
            Debug.Log("player exit");
            playerRb = null;
            player2D = null;
        }
    }
}
