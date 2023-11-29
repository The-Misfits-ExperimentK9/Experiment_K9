using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class Platform : ActivatablePuzzlePiece {

    public List<Vector3> travelLocations;
    public float platformMovementSpeed = 5f;
    public float firstLastWaitTime = 2.0f;

    [SerializeField] GameObject player;
    protected Rigidbody playerRb;
    protected Rigidbody rb;

    protected int currentTargetIndex = 1;
    protected bool isMovingForward = true;
    //  [SerializeField] private bool playerOnPlatform = false;
    protected float distanceToCheck = .1f;
    [SerializeField] private bool unlocked = false;
    protected bool movementStarted = false;

    [SerializeField] private bool unlockedByPlayerCollision = true;
    //   [SerializeField] private bool dontMoveWithoutPlayer = true;

    protected Vector3 currentTravelTarget;

    protected Vector3 lastPos, firstPos;
    public enum PlatformState {
        Waiting,
        Moving,
    }
    [SerializeField] protected PlatformState state;

    protected void Start() {
        if (travelLocations == null || travelLocations.Count < 2) {
            Debug.LogWarning("Insufficient travel locations provided.");
            return;
        }
        else {
            lastPos = travelLocations[^1];
            firstPos = travelLocations[0];
        }
        state = PlatformState.Waiting;
        rb = GetComponent<Rigidbody>();
    }
    public override void Activate() {
        unlocked = true;
        //StartMoving();
        if (!unlockedByPlayerCollision)
            StartCoroutine(WaitThenMove());
    }

    public override void Deactivate(GameObject caller) {
        unlocked = false;
    }

    protected void FixedUpdate() {
        if (travelLocations == null || travelLocations.Count < 2) {
            Debug.LogWarning("Insufficient travel locations provided.");
            return;
        }

        MovePlatform();


    }

    public void StartMoving() {
        if (unlocked) {
            GetNextTargetLocation();
        }

    }

    protected void MovePlatform() {
        if (playerRb) {
            playerRb.velocity = rb.velocity;
        }
        //if the platform is moving
        if (state == PlatformState.Moving) {
            // var targetPosition = travelLocations[currentTargetIndex];
            //check how close it is to the target
            //reached a destination
            if (GotToDestination()) {
                //end pos
                if (Vector3.Distance(currentTravelTarget, lastPos) < .1f) {
                    StartCoroutine(WaitThenMove());
                    isMovingForward = false;
                    currentTargetIndex--;
                }
                //start pos
                else if (Vector3.Distance(currentTravelTarget, firstPos) < .1f) {
                    StartCoroutine(WaitThenMove());
                    isMovingForward = true;
                    currentTargetIndex++;
                }
                //middle point
                else {
                    if (isMovingForward)
                        currentTargetIndex++;
                    else
                        currentTargetIndex--;
                    GetNextTargetLocation();
                }
            }
            //move the platform if not at a destination
            else {



            }
        }
    }
    protected virtual bool GotToDestination() {
        return (currentTravelTarget - transform.position).sqrMagnitude <= distanceToCheck;
    }

    protected virtual void GetNextTargetLocation() {
        currentTravelTarget = travelLocations[currentTargetIndex];
        var moveDirection = (currentTravelTarget - transform.position).normalized;
        var velocity = platformMovementSpeed * moveDirection;
        rb.velocity = velocity; // set the Rigidbody's velocity to move the platform
        
        state = PlatformState.Moving;
    }

    protected IEnumerator WaitThenMove() {
        movementStarted = true;
        //Debug.Log("WaitThenMove");
        state = PlatformState.Waiting;
        rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(firstLastWaitTime);
        
        GetNextTargetLocation();
        yield return null;
    }


    protected void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == LayerInfo.PLAYER) {
            if (player) return;
            
            if (!unlocked && unlockedByPlayerCollision) {
                unlocked = true;

            }
            //  playerOnPlatform = true;
            player = collision.gameObject;
            playerRb = player.GetComponent<Rigidbody>();
            if (!movementStarted)
                StartCoroutine(WaitThenMove());
        }
    }
    protected void OnCollisionExit(Collision collision) {
        if (collision.gameObject.layer == LayerInfo.PLAYER) {
            player = null;
            playerRb = null;

        }
    }
}

