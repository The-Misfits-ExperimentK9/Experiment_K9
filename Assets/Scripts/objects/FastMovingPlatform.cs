using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastMovingPlatform : Platform
{
    bool movingUp = true;

    protected override bool GotToDestination() {
        return movingUp ? transform.position.y >= currentTravelTarget.y : transform.position.y <= currentTravelTarget.y;

    }
    protected override void GetNextTargetLocation() {
        currentTravelTarget = travelLocations[currentTargetIndex];
        var moveDirection = (currentTravelTarget - transform.position).normalized;
        if (Mathf.Sign(currentTravelTarget.y - transform.position.y) > 0) {
            movingUp = true;
        }
        else {
            movingUp = false;
        }
        var velocity = platformMovementSpeed * moveDirection;
        rb.velocity = velocity; // set the Rigidbody's velocity to move the platform
        if (playerRb) {
            playerRb.velocity += velocity;
        }
        state = PlatformState.Moving;
    }
}
