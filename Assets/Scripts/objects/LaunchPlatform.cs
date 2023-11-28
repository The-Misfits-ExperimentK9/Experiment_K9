using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPlatform : MonoBehaviour
{
    [SerializeField] float launchForce = 300f;
    [Space(10)]
    [SerializeField] GameObject player;
    Rigidbody playerRb;
    StarterAssetsInputs playerInput;

    

    private void Update() {
        if (player) {
            if (playerInput.jump)
                LaunchPlayer();
        }
    }
    void LaunchPlayer() {
        Debug.Log("launching player");
        playerRb.velocity = Vector3.up * launchForce;
    }

    protected void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == LayerInfo.PLAYER) {
            if (player) return;

            //  playerOnPlatform = true;
            player = collision.gameObject;
            playerRb = player.GetComponent<Rigidbody>();
            playerInput = player.GetComponent<StarterAssetsInputs>();


        }
    }
    protected void OnCollisionExit(Collision collision) {
        if (collision.gameObject.layer == LayerInfo.PLAYER) {
            player = null;
            playerRb = null;

        }
    }
}
