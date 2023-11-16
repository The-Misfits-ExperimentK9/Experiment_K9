using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerWall : WallBehaviour {
    protected Rigidbody playerRb;
    protected MovementController_2D player2D;
    public float PlayerMoveForceAmount;
    [SerializeField] protected Vector3 playerMoveDirection = Vector3.zero;

    protected virtual void Start() {
        if (playerMoveDirection == Vector3.zero) {
            playerMoveDirection = transform.forward;
        }
        else if (Mathf.Approximately(playerMoveDirection.magnitude, 1)) {
            playerMoveDirection = playerMoveDirection.normalized;
        }
    }

    protected virtual void FixedUpdate() {
        MovePlayer();
    }
    
    protected virtual void MovePlayer() {
        if (playerRb != null) {

            if (player2D.Is2DPlayerActive) {
                playerRb.AddForce(playerMoveDirection * PlayerMoveForceAmount);
            }
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
        if (collision.gameObject.layer == LayerInfo.PLAYER) {
            playerRb = null;
            player2D = null;
        }
    }
}
