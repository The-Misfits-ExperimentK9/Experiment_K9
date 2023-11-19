using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWallConfiner : MonoBehaviour {
    public Transform playerTransform; // Reference to the player transform
    public MovementController_2D playerMovementController; // Reference to the player's movement controller
    public float maxDistanceToWall = 5f; // Maximum allowed distance to the wall
    public LayerMask wallLayer; // Layer mask to identify walls

    private Vector3 offsetFromPlayer; // Initial offset from the player

    void Start() {
        // Calculate and store the initial offset from the player
        offsetFromPlayer = transform.position - playerTransform.position;
    }

    void Update() {
        // Update the position based on the player's position and the initial offset
        Vector3 desiredPosition = playerTransform.position + offsetFromPlayer;

        // Raycast to the left
        RaycastHit hit;
        Debug.DrawRay(playerTransform.position, -playerTransform.right * maxDistanceToWall, Color.yellow);
        Debug.DrawRay(playerTransform.position, playerTransform.right * maxDistanceToWall, Color.yellow);

        var rightColliders = Physics.RaycastAll(playerTransform.position, -playerTransform.right, maxDistanceToWall, wallLayer);
        var leftColliders = Physics.RaycastAll(playerTransform.position, playerTransform.right, maxDistanceToWall, wallLayer);

        var currentWall = playerMovementController.GetCurrentWall();
        GameObject currentWallGameobject;
        if (currentWall) {
            currentWallGameobject = currentWall.gameObject;
        }
        else {
            currentWallGameobject = PlayerBehaviour.Instance.playerDimensionController.CurrentProjectionSurface.gameObject;
        }

        foreach (var rayCastHit in rightColliders) {
            if (rayCastHit.collider.gameObject == currentWallGameobject) {
                continue;
            }
            float distanceToWall = rayCastHit.distance;
            desiredPosition = (rayCastHit.point + playerTransform.right * maxDistanceToWall) + offsetFromPlayer;
          //  float distancePlayerToWall = Vector3.Distance(playerTransform.position, rayCastHit.point);
          // float distanceDifference = distanceToWall - distancePlayerToWall;
          //  desiredPosition = playerTransform.position + (playerTransform.right * distanceToWall);
            Debug.Log("right: " + desiredPosition);
        }
        foreach (var rayCastHit in leftColliders) {
            if (rayCastHit.collider.gameObject == currentWallGameobject) {
                continue;
            }
            float distanceToWall = rayCastHit.distance;
          //  float distancePlayerToWall = Vector3.Distance(playerTransform.position, rayCastHit.point);
          //  float distanceDifference = distanceToWall - distancePlayerToWall;
          //  desiredPosition = playerTransform.position + (-playerTransform.right * distanceToWall);
            Debug.Log("left: " + desiredPosition);
            desiredPosition = (rayCastHit.point + -playerTransform.right * maxDistanceToWall) + offsetFromPlayer;
        }
        transform.position = desiredPosition;

        //if (Physics.Raycast(playerTransform.position, playerTransform.right, out hit, maxDistanceToWall, wallLayer)) {
        //    Debug.Log("Hit left");   
        //    float distanceToWall = hit.distance;
        //    desiredPosition = playerTransform.position + (playerTransform.right * distanceToWall);
        //}
        //// Raycast to the right

        //else if (Physics.Raycast(playerTransform.position, -playerTransform.right, out hit, maxDistanceToWall, wallLayer)) {
        //    Debug.Log("Hit right");
        //    float distanceToWall = hit.distance;
        //    desiredPosition = playerTransform.position + (-playerTransform.right * distanceToWall);
        //}

        //// Set the position of the follow target
        //transform.position = desiredPosition;
    }
}
